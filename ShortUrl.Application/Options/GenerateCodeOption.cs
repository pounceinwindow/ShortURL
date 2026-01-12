using System.Security.Cryptography;
using System.Text;

namespace ShortUrl.Application.Options;

public class GenerateCodeOption
{
    public static string GenerateCode(int len)
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Span<byte> bytes = stackalloc byte[len];
        RandomNumberGenerator.Fill(bytes);
        var sb = new StringBuilder(len);
        for (var i = 0; i < len; i++) sb.Append(alphabet[bytes[i] % alphabet.Length]);
        return sb.ToString();
    }
}