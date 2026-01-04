using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using URLShortener.Data;
using URLShortener.DTO;
using URLShortener.Entities;
using URLShortener.Options;

namespace URLShortener.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {

        app.MapPost("/auth/login", ([FromBody] LoginRequestDto loginData, [FromServices] AppDbContext db) =>
        {
            var user = db.Users.FirstOrDefault(u => u.Email == loginData.Email && u.Password == loginData.Password);

            if (user is null) return Results.Unauthorized();

            var claims = new List<Claim> { new(ClaimTypes.Name, user.Email) };

            var jwt = new JwtSecurityToken(
                AuthOptions.Issuer,
                AuthOptions.Audience,
                claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromHours(24)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var loginResponse = new LoginResponseDto
            {
                Email = user.Email,
                AccessToken = encodedJwt
            };

            return Results.Ok(loginResponse);
        });

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
}