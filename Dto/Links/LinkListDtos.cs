namespace URLShortener.Dto.Links;

public record LinkListItemDto(
    int Id,
    string ShortCode,
    string OriginalUrl,
    DateTime CreatedAt,
    int Clicks,
    string ShortUrl
);

public record LinksListResponseDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<LinkListItemDto> Items
);

public record ClickRowDto(
    DateTime Timestamp,
    string Browser,
    string DeviceType,
    string Referer,
    string Country,
    string City,
    string IpAddress
);

public record DayClicksDto(DateTime Day, int Clicks);

public record LinkStatsResponseDto(
    string ShortCode,
    string OriginalUrl,
    int TotalClicks,
    int Last24h,
    DateTime? LastClick,
    IReadOnlyList<ClickRowDto> Recent
);