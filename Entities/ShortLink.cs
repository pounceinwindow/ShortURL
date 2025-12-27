using System.ComponentModel.DataAnnotations;

namespace URLShortener.Entities;

public class ShortLink
{
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    public string ShortCode { get; set; }

    [Required]
    public string OriginalUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; }

    public User User { get; set; }
    
    public List<Click> Clicks { get; set; } = new ();
}