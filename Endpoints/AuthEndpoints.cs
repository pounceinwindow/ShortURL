using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using URLShortener.Data;
using URLShortener.DTO;
using URLShortener.Entities;
using URLShortener.Options;
using URLShortener.Services;

namespace URLShortener.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", ([FromBody] LoginRequestDto loginData, [FromServices] AppDbContext db, [FromServices] JwtTokenService tokens) =>
        {
            var user = db.Users.FirstOrDefault(u => u.Email == loginData.Email && u.Password == loginData.Password);

            if (user is null) return Results.Unauthorized();

            var encodedJwt = tokens.CreateToken(user.Email);

            var loginResponse = new LoginResponseDto
            {
                Email = user.Email,
                AccessToken = encodedJwt
            };

            return Results.Ok(loginResponse);
        });

        app.MapGet("/auth/google", (HttpContext ctx, IConfiguration cfg, [FromQuery] string? returnUrl) =>
            {
                if (!IsGoogleConfigured(cfg))
                    return Results.Problem("Google OAuth is not configured on the server.", statusCode: StatusCodes.Status501NotImplemented);

                var safeReturnUrl = SanitizeReturnUrl(returnUrl) ?? "/create_link.html";

                var props = new AuthenticationProperties
                {
                    RedirectUri = $"/auth/google/complete?returnUrl={Uri.EscapeDataString(safeReturnUrl)}"
                };

                return Results.Challenge(props, new[] { "Google" });
            })
            .AllowAnonymous();

        app.MapGet("/auth/google/complete", async (
                HttpContext ctx,
                AppDbContext db,
                JwtTokenService tokens,
                [FromQuery] string? returnUrl) =>
            {
                var auth = await ctx.AuthenticateAsync("ExternalCookie");
                if (!auth.Succeeded || auth.Principal is null)
                    return Results.Unauthorized();

                var email = auth.Principal.FindFirstValue(ClaimTypes.Email)
                            ?? auth.Principal.FindFirstValue(ClaimTypes.Name)
                            ?? auth.Principal.Identity?.Name;

                if (string.IsNullOrWhiteSpace(email))
                    return Results.BadRequest(new ErrorResponseDto { Errors = ["Google did not return user email"] });

                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user is null)
                {
                    user = new User
                    {
                        Email = email,
                        Password = Guid.NewGuid().ToString("N")
                    };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }

                var jwt = tokens.CreateToken(email);
                await ctx.SignOutAsync("ExternalCookie");

                var safeReturnUrl = SanitizeReturnUrl(returnUrl) ?? "/create_link.html";
                ctx.Response.Headers["Cache-Control"] = "no-store, no-cache";
                return Results.Text(BuildTokenHtml(jwt, safeReturnUrl), "text/html; charset=utf-8");
            })
            .AllowAnonymous();

        app.MapPost("/auth/create_user",
            ([FromBody] CreateUserRequestDto createUserData, [FromServices] AppDbContext db) =>
            {
                var user = db.Users.FirstOrDefault(u => u.Email == createUserData.Email);

                if (user is not null)
                    return Results.Conflict(new ErrorResponseDto
                    {
                        Errors = ["Пользователь с таким email уже существует"]
                    });

                var newUser = new User
                {
                    Email = createUserData.Email,
                    Password = createUserData.Password
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                return Results.Ok(new CreateUserResponseDto
                {
                    Id = newUser.Id,
                    Email = newUser.Email,
                    Password = newUser.Password,
                    CreatedAt = newUser.CreatedAt
                });
            });
    }

    private static bool IsGoogleConfigured(IConfiguration cfg)
    {
        var clientId = cfg[$"{GoogleAuthOptions.SectionPath}:ClientId"];
        var clientSecret = cfg[$"{GoogleAuthOptions.SectionPath}:ClientSecret"];
        return !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret);
    }

    private static string? SanitizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;
        return returnUrl.StartsWith('/') && !returnUrl.StartsWith("//") ? returnUrl : null;
    }

    private static string BuildTokenHtml(string token, string returnUrl)
    {
        var t = JavaScriptEncoder.Default.Encode(token);
        var r = JavaScriptEncoder.Default.Encode(returnUrl);
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>Signing in…</title>
</head>
<body>
  <script>
    try {{ localStorage.setItem('token', '{t}'); }} catch (e) {{}}
    window.location.replace('{r}');
  </script>
</body>
</html>";
    }
}
