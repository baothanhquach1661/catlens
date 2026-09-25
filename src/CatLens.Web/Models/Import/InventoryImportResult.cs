namespace CatLens.Web.Models.Import;

public sealed record InventoryImportResult(
    int CsvRowCount,
    int VariantCount,
    int InventoryLevelCount,
    int NotStockedCount);