using System.ComponentModel.DataAnnotations;

namespace URLShortener.DTO;

public class CreateLinkResponse
{
    [Required] public int Id { get; set; }

    [Required] public string ShortCode { get; set; }

    [Required] public string ShortUrl { get; set; }
}