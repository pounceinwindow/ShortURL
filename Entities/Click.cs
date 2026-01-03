using System.ComponentModel.DataAnnotations;

namespace URLShortener.Entities;

public class Click
{
    public int Id { get; set; }

    public int ShortLinkId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(45)] public string IpAddress { get; set; }

    public string UserAgent { get; set; }

    public string Referer { get; set; }

    [StringLength(100)] public string Country { get; set; }

    [StringLength(100)] public string City { get; set; }

    [StringLength(50)] public string DeviceType { get; set; }

    [StringLength(50)] public string Browser { get; set; }
}