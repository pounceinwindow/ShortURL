using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace URLShortener.Options;

public static class AuthOptions
{
    public const string Issuer = "MyAuthServer";
    public const string Audience = "MyAuthClient";
    private const string Key = "Q7xvB7fQk3b8n9eWm2s5c0p1Jf4r8t6yZ0a3d9h2k7m1p5v8X2n4s6d8";

    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    }
}