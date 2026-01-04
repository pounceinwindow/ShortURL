using MaxMind.Db;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using URLShortener.Data;
using URLShortener.Endpoints;
using URLShortener.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DatabaseReader>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var cfg = sp.GetRequiredService<IConfiguration>();

    var path = cfg["GeoIp:CityDbPath"];

    if (string.IsNullOrWhiteSpace(path))
        path = Path.Combine(env.ContentRootPath, "Data", "GeoLite2-City.mmdb");

    if (!Path.IsPathRooted(path))
        path = Path.Combine(env.ContentRootPath, path);

    if (!File.Exists(path))
        throw new FileNotFoundException($"GeoIP database not found: {path}");

    return new DatabaseReader(path, FileAccessMode.MemoryMapped);
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = AuthOptions.Audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey()
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "login.html" }
});
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapLinkEndpoints();

app.Run();
