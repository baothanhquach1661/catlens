using CsvHelper.Configuration.Attributes;

namespace CatLens.Web.Models.Reviews;

public sealed class HighInventoryExportRow
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

    [Name("Location")]
    public string Location { get; init; } = string.Empty;

    [Name("On Hand Quantity")]
    public int OnHandQuantity { get; init; }

    [Name("Warning Threshold")]
    public int WarningThreshold { get; init; }

    [Name("Amount Above Threshold")]
    public int AmountAboveThreshold { get; init; }

    [Name("Issue")]
    public string Issue { get; init; } = string.Empty;
}