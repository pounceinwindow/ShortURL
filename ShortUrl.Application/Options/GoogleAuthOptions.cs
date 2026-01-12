namespace URLShortener.Options;

public sealed class GoogleAuthOptions
{
    public const string SectionPath = "Authentication:Google";

    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    public string CallbackPath { get; set; } = "/auth/google-callback";
}