namespace CatLens.Web.Configuration;

public sealed class CatalogReviewOptions
{
    public const string SectionName = "CatalogReview";

    public int HighInventoryThreshold { get; set; } = 1000;
}