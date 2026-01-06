using MaxMind.Db;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using URLShortener.Data;
using URLShortener.Options;
using URLShortener.Services;

namespace URLShortener.Extensions;

public static class ServiceCollectionExtensions
{
    private const string ExternalCookieScheme = "ExternalCookie";
    private const string GoogleScheme = "Google";

    public static IServiceCollection AddAppServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        services.AddSingleton<JwtTokenService>();

        services.AddGeoIp(configuration, env);
        services.AddAppAuthentication(configuration);
        services.AddAppDb(configuration);

        return services;
    }

    private static IServiceCollection AddGeoIp(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        services.AddSingleton<DatabaseReader>(_ =>
        {
            var path = configuration["GeoIp:CityDbPath"];

            if (string.IsNullOrWhiteSpace(path))
                path = Path.Combine(env.ContentRootPath, "Data", "GeoLite2-City.mmdb");

            if (!Path.IsPathRooted(path))
                path = Path.Combine(env.ContentRootPath, path);

            if (!File.Exists(path))
                throw new FileNotFoundException($"GeoIP database not found: {path}");

            return new DatabaseReader(path, FileAccessMode.MemoryMapped);
        });

        return services;
    }

    private static IServiceCollection AddAppDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }

    private static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization();

        var auth = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
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

        var google = new GoogleAuthOptions
        {
            ClientId = configuration[$"{GoogleAuthOptions.SectionPath}:ClientId"],
            ClientSecret = configuration[$"{GoogleAuthOptions.SectionPath}:ClientSecret"],
            CallbackPath = configuration[$"{GoogleAuthOptions.SectionPath}:CallbackPath"] ?? "/auth/google-callback"
        };

        var googleConfigured =
            !string.IsNullOrWhiteSpace(google.ClientId) &&
            !string.IsNullOrWhiteSpace(google.ClientSecret);

        if (googleConfigured)
        {
            auth.AddCookie(ExternalCookieScheme, options =>
            {
                options.Cookie.Name = "ExternalAuth";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = false;
            });

            auth.AddGoogle(GoogleScheme, options =>
            {
                options.SignInScheme = ExternalCookieScheme;
                options.ClientId = google.ClientId!;
                options.ClientSecret = google.ClientSecret!;
                options.CallbackPath = google.CallbackPath;
                options.SaveTokens = true;
            });
        }

        return services;
    }
}
