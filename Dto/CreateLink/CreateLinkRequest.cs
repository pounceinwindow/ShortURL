using System.ComponentModel.DataAnnotations;

namespace URLShortener.DTO;

public class CreateLinkRequest
{
    [Required]
    public string OriginalUrl { get; set; }
    [Required]
    public string ShortCode { get; set; }
}