using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace URLShortener.Options;

public static class AuthOptions
{
    public const string Issuer = "MyAuthServer";     // <-- подставь своё
    public const string Audience = "MyAuthClient";   // <-- подставь своё
    private const string Key = "Q7xvB7fQk3b8n9eWm2s5c0p1Jf4r8t6yZ0a3d9h2k7m1p5v8X2n4s6d8"; // <-- подставь своё (минимум 16-32+ символов)

    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
}