using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using URLShortener.Data;
using URLShortener.Endpoints;
using URLShortener.Options;

var builder = WebApplication.CreateBuilder(args);

var geoDbPath = builder.Configuration["GeoIp:CityDbPath"];
builder.Services.AddSingleton<DatabaseReader?>(_ =>
    !string.IsNullOrWhiteSpace(geoDbPath) && File.Exists(geoDbPath)
        ? new DatabaseReader(geoDbPath)
        : null
);

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
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();
app.MapAuthEndpoints();
app.MapLinkEndpoints(); 


app.Run();
