using System.ComponentModel.DataAnnotations;

namespace URLShortener.DTO;

public class LinkStatsDto
{
    [Required] public int Id { get; set; }

    [Required] public string ShortCode { get; set; }

    [Required] public string ShortUrl { get; set; }

    [Required] public string OriginalUrl { get; set; }

    public int TotalClicks { get; set; }

    public int Last24Hours { get; set; }

    public DateTime? LastClickAt { get; set; }

    public List<LinkStatsClickDto> RecentClicks { get; set; } = new();
}
