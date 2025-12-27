using System.ComponentModel.DataAnnotations;

namespace URLShortener.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; }
    
    [Required]
    [StringLength(256)]
    public string Password { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ShortLink> ShortLinks { get; set; } = new ();
}