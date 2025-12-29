using System.Net;
using MaxMind.GeoIP2;

namespace URLShortener.GeoIp;

public class GetGeoIp
{
    
    public static (string country, string city) ResolveGeo(DatabaseReader? reader, string? ip, string acceptLangHeader)
    {
        if (reader == null || string.IsNullOrWhiteSpace(ip) || !IPAddress.TryParse(ip, out var addr) || !IsPublicIp(addr))
            return ("", "");

        try
        {
            var resp = reader.City(addr);
            // язык интерфейса — возьмём ru, если он первый в Accept-Language
            var lang = (acceptLangHeader ?? "").StartsWith("ru", StringComparison.OrdinalIgnoreCase) ? "ru" : "en";

            string country = "";
            if (resp.Country?.Names != null && resp.Country.Names.TryGetValue(lang, out var cName))
                country = cName;
            else
                country = resp.Country?.Name ?? resp.Country?.IsoCode ?? "";

            string city = "";
            if (resp.City?.Names != null && resp.City.Names.TryGetValue(lang, out var ct))
                city = ct;
            else
                city = resp.City?.Name ?? "";

            return (country ?? "", city ?? "");
        }
        catch
        {
            return ("", "");
        }
    }

    public static bool IsPublicIp(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip)) return false;

        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var b = ip.GetAddressBytes();
            // 10.0.0.0/8
            if (b[0] == 10) return false;
            // 172.16.0.0/12
            if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return false;
            // 192.168.0.0/16
            if (b[0] == 192 && b[1] == 168) return false;
            // 169.254.0.0/16 (link-local)
            if (b[0] == 169 && b[1] == 254) return false;
            // 127.0.0.0/8 (loopback уже проверили)
        }
        else if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal) return false;
        }
        return true;
    }
}