using System.Security.Claims;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using ShortUrl.Application.Options;
using UAParser;
using URLShortener.Data;
using URLShortener.DTO;
using URLShortener.Dto.Links;
using URLShortener.Entities;
using URLShortener.GeoIp;

namespace URLShortener.Endpoints;

public static class LinkEndpoints
{
    public static void MapLinkEndpoints(this WebApplication app)
    {
        var links = app.MapGroup("/links").RequireAuthorization();

        links.MapPost("/", static async (
            [FromBody] CreateLinkRequest dto,
            AppDbContext db,
            HttpContext ctx,
            ClaimsPrincipal user) =>
        {
            if (string.IsNullOrWhiteSpace(dto.OriginalUrl) ||
                !Uri.TryCreate(dto.OriginalUrl, UriKind.Absolute, out _))
                return Results.BadRequest(new { message = "originalUrl is invalid" });

            var email = user.Identity?.Name;
            var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (dbUser is null) return Results.Unauthorized();

            var code = (dto.ShortCode ?? "").Trim();
            if (string.IsNullOrEmpty(code))
                for (var i = 0; i < 8; i++)
                {
                    code = GenerateCodeOption.GenerateCode(7);
                    if (!await db.ShortLinks.AnyAsync(x => x.ShortCode == code)) break;
                    if (i == 7) return Results.StatusCode(500);
                }
            else if (await db.ShortLinks.AnyAsync(x => x.ShortCode == code))
                return Results.Conflict(new { message = "shortCode already exists" });

            var link = new ShortLink
            {
                ShortCode = code,
                OriginalUrl = dto.OriginalUrl,
                UserId = dbUser.Id
            };

            db.ShortLinks.Add(link);
            await db.SaveChangesAsync();

            var shortUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{code}";
            return Results.Ok(new CreateLinkResponse { Id = link.Id, ShortCode = link.ShortCode, ShortUrl = shortUrl });
        });

        links.MapGet("/", static async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? q,
            AppDbContext db,
            HttpContext ctx,
            ClaimsPrincipal user) =>
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize is <= 0 or > 100 ? 20 : pageSize;

            var email = user.Identity?.Name;
            var dbUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            if (dbUser is null) return Results.Unauthorized();

            var query = db.ShortLinks.AsNoTracking().Where(l => l.UserId == dbUser.Id);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                query = query.Where(l => l.ShortCode.Contains(s) || l.OriginalUrl.Contains(s));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .GroupJoin(
                    db.Clicks.AsNoTracking(),
                    l => l.Id,
                    c => c.ShortLinkId,
                    (l, clicks) => new { l, clicksCount = clicks.Count() }
                )
                .Select(x => new LinkListItemDto(
                    x.l.Id,
                    x.l.ShortCode,
                    x.l.OriginalUrl,
                    x.l.CreatedAt,
                    x.clicksCount,
                    $"{ctx.Request.Scheme}://{ctx.Request.Host}/{x.l.ShortCode}"
                ))
                .ToListAsync();

            return Results.Ok(new LinksListResponseDto(page, pageSize, total, items));
        });

        app.MapGet("/{code}/qr", static async (
            [FromRoute] string code,
            [FromQuery] int? size,
            AppDbContext db,
            HttpContext ctx) =>
        {
            var link = await db.ShortLinks.FirstOrDefaultAsync(l => l.ShortCode == code);
            if (link is null) return Results.NotFound();

            var url = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{code}";
            var data = new QRCodeGenerator().CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

            var targetPx = Math.Clamp(size ?? 256, 128, 4096);
            var modules = data.ModuleMatrix.Count;
            var ppm = Math.Max(1, targetPx / modules);

            var png = new PngByteQRCode(data).GetGraphic(ppm);
            return Results.File(png, "image/png");
        }).AllowAnonymous();

        app.MapGet(
            "/{code:regex(^(?!api$|auth$|links$|swagger$|docs$|favicon\\.ico$|robots\\.txt$)[A-Za-z0-9]{{4,10}}$)}",
            static async (
                [FromRoute] string code,
                AppDbContext db,
                DatabaseReader? geo,
                HttpRequest req) =>
            {
                var link = await db.ShortLinks.FirstOrDefaultAsync(l => l.ShortCode == code);
                if (link is null) return Results.NotFound();

                var ip = GetIp.GetClientIp(req);
                var (country, city) = GetGeoIp.ResolveGeo(geo, ip, req.Headers["Accept-Language"]);

                var ua = req.Headers["User-Agent"].ToString();
                var referer = req.Headers["Referer"].ToString();
                var client = Parser.GetDefault().Parse(ua);
                var device = DeviceType.InferDeviceType(ua);
                var browser = $"{client.UA.Family} {client.UA.Major}".Trim();

                db.Clicks.Add(new Click
                {
                    ShortLinkId = link.Id,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ip ?? string.Empty,
                    UserAgent = ua ?? string.Empty,
                    Referer = string.IsNullOrWhiteSpace(referer) ? string.Empty : referer,
                    Browser = string.IsNullOrWhiteSpace(browser) ? "Unknown" : browser,
                    DeviceType = string.IsNullOrWhiteSpace(device) ? "desktop" : device,
                    Country = string.IsNullOrWhiteSpace(country) ? "Unknown" : country,
                    City = string.IsNullOrWhiteSpace(city) ? "Unknown" : city
                });

                await db.SaveChangesAsync();
                return Results.Redirect(link.OriginalUrl);
            }).AllowAnonymous();

        links.MapGet("/{code}/stats", static async (
            [FromRoute] string code,
            AppDbContext db,
            ClaimsPrincipal user) =>
        {
            var email = user.Identity?.Name;
            var dbUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            if (dbUser is null) return Results.Unauthorized();

            var link = await db.ShortLinks.AsNoTracking()
                .FirstOrDefaultAsync(l => l.UserId == dbUser.Id && l.ShortCode == code);
            if (link is null) return Results.NotFound();

            var linkId = link.Id;
            var now = DateTime.UtcNow;
            var since24h = now.AddHours(-24);

            var totalClicks = await db.Clicks.AsNoTracking().CountAsync(c => c.ShortLinkId == linkId);
            var last24h = await db.Clicks.AsNoTracking()
                .CountAsync(c => c.ShortLinkId == linkId && c.Timestamp >= since24h);

            var lastClick = await db.Clicks.AsNoTracking()
                .Where(c => c.ShortLinkId == linkId)
                .OrderByDescending(c => c.Timestamp)
                .Select(c => (DateTime?)c.Timestamp)
                .FirstOrDefaultAsync();

            var recent = await db.Clicks.AsNoTracking()
                .Where(c => c.ShortLinkId == linkId)
                .OrderByDescending(c => c.Timestamp)
                .Take(25)
                .Select(c => new ClickRowDto(
                    c.Timestamp,
                    c.Browser,
                    c.DeviceType,
                    c.Referer,
                    c.Country,
                    c.City,
                    c.IpAddress
                ))
                .ToListAsync();

            return Results.Ok(new LinkStatsResponseDto(
                link.ShortCode,
                link.OriginalUrl,
                totalClicks,
                last24h,
                lastClick,
                recent
            ));
        });
    }

}