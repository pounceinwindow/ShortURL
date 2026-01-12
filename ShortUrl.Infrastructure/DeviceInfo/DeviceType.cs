namespace URLShortener.GeoIp;

public class DeviceType
{
    public static string InferDeviceType(string ua)
    {
        var u = ua.ToLowerInvariant();
        if (u.Contains("ipad") || u.Contains("tablet")) return "tablet";
        if (u.Contains("mobi") || u.Contains("iphone") || u.Contains("android")) return "mobile";
        return "desktop";
    }
}