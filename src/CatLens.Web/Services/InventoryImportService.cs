using System.Globalization;
using CatLens.Web.Data;
using CatLens.Web.Models;
using CatLens.Web.Models.Import;
using CsvHelper;
using Microsoft.EntityFrameworkCore;

namespace CatLens.Web.Services;

public sealed class InventoryImportService
{
    private readonly CatLensDbContext _dbContext;

    public InventoryImportService(CatLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InventoryImportResult> ImportAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default)
    {
        var rows = await ReadRowsAsync(
            csvStream,
            cancellationToken);

        if (rows.Count == 0)
        {
            throw new InvalidDataException(
                "The CSV file does not contain any inventory rows.");
        }

        var variants = BuildVariants(
            rows,
            out var notStockedCount);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await _dbContext.InventoryLevels.ExecuteDeleteAsync(
            cancellationToken);

        await _dbContext.Variants.ExecuteDeleteAsync(
            cancellationToken);

        await _dbContext.Variants.AddRangeAsync(
            variants,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new InventoryImportResult(
            CsvRowCount: rows.Count,
            VariantCount: variants.Count,
            InventoryLevelCount: variants.Sum(
                variant => variant.InventoryLevels.Count),
            NotStockedCount: notStockedCount);
    }

    private static async Task<List<InventoryCsvRow>> ReadRowsAsync(
        Stream csvStream,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(csvStream);

        using var csv = new CsvReader(
            reader,
            CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<InventoryCsvRowMap>();

        var rows = new List<InventoryCsvRow>();

        await foreach (
            var row in csv.GetRecordsAsync<InventoryCsvRow>(
                cancellationToken))
        {
            rows.Add(row);
        }

        return rows;
    }

    private static List<Variant> BuildVariants(
        IReadOnlyCollection<InventoryCsvRow> rows,
        out int notStockedCount)
    {
        notStockedCount = 0;

        var groupedRows = rows.GroupBy(row => new VariantKey(
            Normalize(row.Handle),
            Normalize(row.Option1Value),
            Normalize(row.Option2Value),
            Normalize(row.Option3Value)));

        var variants = new List<Variant>();

        foreach (var group in groupedRows)
        {
            if (string.IsNullOrWhiteSpace(group.Key.Handle))
            {
                throw new InvalidDataException(
                    "A CSV row contains a blank Handle.");
            }

            var skuValues = group
                .Select(row => Normalize(row.Sku))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (skuValues.Count > 1)
            {
                throw new InvalidDataException(
                    $"Variant '{group.Key.Handle}' contains " +
                    "different SKU values across locations.");
            }

            var title = group
                .Select(row => Normalize(row.Title))
                .FirstOrDefault(value =>
                    !string.IsNullOrWhiteSpace(value));

            var sku = skuValues.SingleOrDefault();

            var variant = new Variant
            {
                Handle = group.Key.Handle,
                Title = string.IsNullOrWhiteSpace(title)
                    ? group.Key.Handle
                    : title,
                Option1Value = group.Key.Option1Value,
                Option2Value = group.Key.Option2Value,
                Option3Value = group.Key.Option3Value,
                Sku = string.IsNullOrWhiteSpace(sku)
                    ? null
                    : sku
            };

            var locations = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var row in group)
            {
                var location = Normalize(row.Location);

                if (string.IsNullOrWhiteSpace(location))
                {
                    throw new InvalidDataException(
                        $"Variant '{group.Key.Handle}' " +
                        "contains a blank Location.");
                }

                if (!locations.Add(location))
                {
                    throw new InvalidDataException(
                        $"Variant '{group.Key.Handle}' has more than " +
                        $"one row for location '{location}'.");
                }

                var onHandQuantity = ParseOnHandQuantity(
                    row.OnHandCurrent,
                    ref notStockedCount);

                variant.InventoryLevels.Add(
                    new InventoryLevel
                    {
                        Location = location,
                        OnHandQuantity = onHandQuantity
                    });
            }

            variants.Add(variant);
        }

        return variants;
    }

    private static int? ParseOnHandQuantity(
        string? value,
        ref int notStockedCount)
    {
        var normalizedValue = Normalize(value);

        if (string.Equals(
                normalizedValue,
                "not stocked",
                StringComparison.OrdinalIgnoreCase))
        {
            notStockedCount++;
            return null;
        }

        if (int.TryParse(
                normalizedValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var quantity))
        {
            return quantity;
        }

        throw new InvalidDataException(
            $"Inventory value '{normalizedValue}' " +
            "is not a valid whole number or 'not stocked'.");
    }

    private static string Normalize(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private sealed record VariantKey(
        string Handle,
        string Option1Value,
        string Option2Value,
        string Option3Value);
}
