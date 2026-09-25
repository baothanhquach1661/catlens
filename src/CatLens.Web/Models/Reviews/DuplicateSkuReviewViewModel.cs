namespace CatLens.Web.Models.Reviews;

public sealed class DuplicateSkuReviewViewModel
{
    public int DuplicateVariantCount { get; init; }

    public List<DuplicateSkuGroupViewModel> Groups { get; init; }
        = new();

    public int DuplicateSkuGroupCount => Groups.Count;
}

public sealed class DuplicateSkuGroupViewModel
{
    public string Sku { get; init; } = string.Empty;

    public List<DuplicateSkuVariantRowViewModel> Variants
    { get; init; } = new();
}

public sealed class DuplicateSkuVariantRowViewModel
{
    public string Sku { get; init; } = string.Empty;

    public string Handle { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Option1Value { get; init; } = string.Empty;

    public string Option2Value { get; init; } = string.Empty;

    public string Option3Value { get; init; } = string.Empty;

    public int LocationCount { get; init; }

    public long TotalOnHand { get; init; }
}