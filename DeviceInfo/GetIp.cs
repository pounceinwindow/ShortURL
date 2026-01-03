using System.Net;

namespace URLShortener.GeoIp;

public class GetIp
{
    public static string? GetClientIp(HttpRequest req)
    {
        // Приоритет X-Forwarded-For (если за прокси), иначе RemoteIpAddress
        var xff = req.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(xff))
            foreach (var raw in xff.Split(','))
            {
                var s = raw.Trim();
                if (IPAddress.TryParse(s, out var addr) && GetGeoIp.IsPublicIp(addr))
                    return s;
            }

        var remote = req.HttpContext.Connection.RemoteIpAddress?.ToString();
        if (IPAddress.TryParse(remote, out var r) && GetGeoIp.IsPublicIp(r)) return remote;
        return remote; // если локалка — вернём как есть (гео не найдём)
    }
}