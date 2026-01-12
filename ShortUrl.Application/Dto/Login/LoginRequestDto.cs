using System.ComponentModel.DataAnnotations;

namespace URLShortener.DTO;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; }

    [Required] [StringLength(256)] public string Password { get; set; }
}