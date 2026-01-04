using System.ComponentModel.DataAnnotations;

namespace URLShortener.DTO;

public class LinkListItemDto
{
    [Required] public int Id { get; set; }

    [Required] public string ShortCode { get; set; }

    [Required] public string ShortUrl { get; set; }

    [Required] public string OriginalUrl { get; set; }

    [Required] public DateTime CreatedAt { get; set; }

    public int TotalClicks { get; set; }

    public DateTime? LastClickAt { get; set; }
}
