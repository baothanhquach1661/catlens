using CsvHelper.Configuration;

namespace CatLens.Web.Models.Import;

public sealed class InventoryCsvRowMap : ClassMap<InventoryCsvRow>
{
    public InventoryCsvRowMap()
    {
        Map(row => row.Handle)
            .Name("Handle");

        Map(row => row.Title)
            .Name("Title");

        Map(row => row.Option1Value)
            .Name("Option1 Value");

        Map(row => row.Option2Value)
            .Name("Option2 Value");

        Map(row => row.Option3Value)
            .Name("Option3 Value");

        Map(row => row.Sku)
            .Name("SKU");

        Map(row => row.Location)
            .Name("Location");

        Map(row => row.OnHandCurrent)
            .Name("On hand (current)");
    }
}