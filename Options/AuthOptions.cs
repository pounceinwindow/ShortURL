using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace URLShortener.Options;

public class AuthOptions
{
    public const string Issuer = "MyAuthServer"; // издатель токена
    public const string Audience = "MyAuthClient"; // потребитель токена
    const string Key = "ThisIsASuperSecretKey123456789012";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => 
        new (Encoding.UTF8.GetBytes(Key));
}
