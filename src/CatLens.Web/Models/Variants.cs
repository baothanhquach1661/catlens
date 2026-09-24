namespace CatLens.Web.Models;

public class Variant
{
    public int Id { get; set; }

    public string Handle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Option1Value { get; set; } = string.Empty;

    public string Option2Value { get; set; } = string.Empty;

    public string Option3Value { get; set; } = string.Empty;

    public string? Sku { get; set; }

    public ICollection<InventoryLevel> InventoryLevels { get; set; }
    = new List<InventoryLevel>();


}