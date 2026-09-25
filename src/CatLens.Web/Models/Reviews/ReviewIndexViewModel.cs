namespace CatLens.Web.Models.Reviews;

public sealed class ReviewIndexViewModel
{
    public int MissingSkuCount { get; init; }

    public int CurrentPage { get; init; }

    public int PageSize { get; init; }

    public int TotalPages { get; init; }

    public List<MissingSkuRowViewModel> MissingSkuVariants
    { get; init; } = new();
}

public sealed class MissingSkuRowViewModel
{
    public string Handle { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Option1Value { get; init; } = string.Empty;

    public string Option2Value { get; init; } = string.Empty;

    public string Option3Value { get; init; } = string.Empty;

    public int LocationCount { get; init; }

    public long TotalOnHand { get; init; }
}