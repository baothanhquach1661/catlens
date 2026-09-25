using CsvHelper.Configuration.Attributes;

namespace CatLens.Web.Models.Reviews;

public sealed class MissingSkuExportRow
{
    [Name("Handle")]
    public string Handle { get; init; } = string.Empty;

    [Name("Title")]
    public string Title { get; init; } = string.Empty;

    [Name("Option 1 Value")]
    public string? Option1Value { get; init; }

    [Name("Option 2 Value")]
    public string? Option2Value { get; init; }

    [Name("Option 3 Value")]
    public string? Option3Value { get; init; }

    [Name("SKU")]
    public string? Sku { get; init; }

    [Name("Location Count")]
    public int LocationCount { get; init; }

    [Name("Total On Hand")]
    public long TotalOnHand { get; init; }

    [Name("Issue")]
    public string Issue { get; init; } = string.Empty;
}