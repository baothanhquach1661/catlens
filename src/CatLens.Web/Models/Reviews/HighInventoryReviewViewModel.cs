namespace CatLens.Web.Models.Reviews;

public sealed class HighInventoryReviewViewModel
{
    public int Threshold { get; init; }

    public int WarningCount { get; init; }

    public int CurrentPage { get; init; }

    public int PageSize { get; init; }

    public int TotalPages { get; init; }

    public List<HighInventoryRowViewModel> Rows { get; init; }
        = new();
}

public sealed class HighInventoryRowViewModel
{
    public string Handle { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Option1Value { get; init; } = string.Empty;

    public string Option2Value { get; init; } = string.Empty;

    public string Option3Value { get; init; } = string.Empty;

    public string? Sku { get; init; }

    public string Location { get; init; } = string.Empty;

    public int OnHandQuantity { get; init; }
}