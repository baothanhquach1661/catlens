namespace CatLens.Web.Models;

public class InventoryLevel
{
    public int Id { get; set; }

    public int VariantId { get; set; }

    public string Location { get; set; } = string.Empty;

    public int? OnHandQuantity { get; set; }

    public Variant Variant { get; set; } = null!;
}