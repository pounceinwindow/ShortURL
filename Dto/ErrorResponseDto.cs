namespace URLShortener.DTO;

public class ErrorResponseDto
{
    public bool Success { get; set; } = false;
    public string[] Errors { get; set; }
}