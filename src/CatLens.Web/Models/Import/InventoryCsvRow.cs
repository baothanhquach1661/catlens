namespace CatLens.Web.Models.Import;

public sealed class InventoryCsvRow
{
    public string Handle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Option1Value { get; set; } = string.Empty;

    public string Option2Value { get; set; } = string.Empty;

    public string Option3Value { get; set; } = string.Empty;

    public string? Sku { get; set; }

    public string Location { get; set; } = string.Empty;

    public string OnHandCurrent { get; set; } = string.Empty;
}