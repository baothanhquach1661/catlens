namespace CatLens.Web.Models.Catalog;

public sealed class CatalogIndexViewModel
{
    public int TotalVariants { get; init; }

    public int TotalInventoryLevels { get; init; }

    public int MissingSkuVariants { get; init; }

    public List<CatalogVariantRowViewModel> Variants { get; init; }
        = new();
}

public sealed class CatalogVariantRowViewModel
{
    public string Handle { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Option1Value { get; init; } = string.Empty;

    public string Option2Value { get; init; } = string.Empty;

    public string Option3Value { get; init; } = string.Empty;

    public string? Sku { get; init; }

    public int LocationCount { get; init; }

    public int StockedLocationCount { get; init; }

    public long TotalOnHand { get; init; }
}