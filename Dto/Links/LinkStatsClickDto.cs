namespace URLShortener.DTO;

public class LinkStatsClickDto
{
    public DateTime Timestamp { get; set; }

    public string Browser { get; set; }

    public string Referer { get; set; }

    public string IpHash { get; set; }
}
