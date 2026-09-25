using System.Globalization;
using System.Text;
using CatLens.Web.Configuration;
using CatLens.Web.Data;
using CatLens.Web.Models.Reviews;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CatLens.Web.Controllers;

public class ReviewsController : Controller
{
    private const int PageSize = 50;

    private readonly CatLensDbContext _dbContext;
    private readonly int _highInventoryThreshold;

    public ReviewsController(
        CatLensDbContext dbContext,
        IOptions<CatalogReviewOptions> reviewOptions)
    {
        _dbContext = dbContext;

        _highInventoryThreshold =
            reviewOptions.Value.HighInventoryThreshold;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);

        var missingSkuQuery = _dbContext.Variants
            .AsNoTracking()
            .Where(variant =>
                variant.Sku == null ||
                variant.Sku == string.Empty);

        var missingSkuCount =
            await missingSkuQuery.CountAsync(cancellationToken);

        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling(
                missingSkuCount / (double)PageSize));

        page = Math.Min(page, totalPages);

        var missingSkuVariants = await missingSkuQuery
            .OrderBy(variant => variant.Title)
            .ThenBy(variant => variant.Handle)
            .ThenBy(variant => variant.Option1Value)
            .ThenBy(variant => variant.Option2Value)
            .ThenBy(variant => variant.Option3Value)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(variant => new MissingSkuRowViewModel
            {
                Handle = variant.Handle,
                Title = variant.Title,
                Option1Value = variant.Option1Value,
                Option2Value = variant.Option2Value,
                Option3Value = variant.Option3Value,

                LocationCount =
                    variant.InventoryLevels.Count,

                TotalOnHand =
                    variant.InventoryLevels
                        .Sum(level =>
                            (long?)level.OnHandQuantity) ?? 0
            })
            .ToListAsync(cancellationToken);

        var model = new ReviewIndexViewModel
        {
            MissingSkuCount = missingSkuCount,
            CurrentPage = page,
            PageSize = PageSize,
            TotalPages = totalPages,
            MissingSkuVariants = missingSkuVariants
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DuplicateSkus(
        CancellationToken cancellationToken = default)
    {
        var duplicateSkuKeys = await _dbContext.Variants
            .AsNoTracking()
            .Where(variant =>
                variant.Sku != null &&
                variant.Sku != string.Empty)
            .GroupBy(variant => variant.Sku!.ToUpper())
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToListAsync(cancellationToken);

        var duplicateRows = await _dbContext.Variants
            .AsNoTracking()
            .Where(variant =>
                variant.Sku != null &&
                duplicateSkuKeys.Contains(
                    variant.Sku!.ToUpper()))
            .OrderBy(variant => variant.Sku)
            .ThenBy(variant => variant.Title)
            .ThenBy(variant => variant.Handle)
            .Select(variant =>
                new DuplicateSkuVariantRowViewModel
                {
                    Sku = variant.Sku!,
                    Handle = variant.Handle,
                    Title = variant.Title,
                    Option1Value = variant.Option1Value,
                    Option2Value = variant.Option2Value,
                    Option3Value = variant.Option3Value,

                    LocationCount =
                        variant.InventoryLevels.Count,

                    TotalOnHand =
                        variant.InventoryLevels
                            .Sum(level =>
                                (long?)level.OnHandQuantity) ?? 0
                })
            .ToListAsync(cancellationToken);

        var groups = duplicateRows
            .GroupBy(
                row => row.Sku,
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                group => group.Key,
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new DuplicateSkuGroupViewModel
            {
                Sku = group.First().Sku,

                Variants = group
                    .OrderBy(row => row.Title)
                    .ThenBy(row => row.Handle)
                    .ToList()
            })
            .ToList();

        var model = new DuplicateSkuReviewViewModel
        {
            DuplicateVariantCount = duplicateRows.Count,
            Groups = groups
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> HighInventory(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);

        var highInventoryQuery = _dbContext.InventoryLevels
            .AsNoTracking()
            .Where(level =>
                level.OnHandQuantity.HasValue &&
                level.OnHandQuantity.Value >
                    _highInventoryThreshold);

        var warningCount =
            await highInventoryQuery.CountAsync(
                cancellationToken);

        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling(
                warningCount / (double)PageSize));

        page = Math.Min(page, totalPages);

        var rows = await highInventoryQuery
            .OrderByDescending(level =>
                level.OnHandQuantity)
            .ThenBy(level => level.Location)
            .ThenBy(level => level.Variant.Title)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(level => new HighInventoryRowViewModel
            {
                Handle = level.Variant.Handle,
                Title = level.Variant.Title,

                Option1Value =
                    level.Variant.Option1Value,

                Option2Value =
                    level.Variant.Option2Value,

                Option3Value =
                    level.Variant.Option3Value,

                Sku = level.Variant.Sku,
                Location = level.Location,

                OnHandQuantity =
                    level.OnHandQuantity ?? 0
            })
            .ToListAsync(cancellationToken);

        var model = new HighInventoryReviewViewModel
        {
            Threshold = _highInventoryThreshold,
            WarningCount = warningCount,
            CurrentPage = page,
            PageSize = PageSize,
            TotalPages = totalPages,
            Rows = rows
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportMissingSkus(
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.Variants
            .AsNoTracking()
            .Where(variant =>
                variant.Sku == null ||
                variant.Sku == string.Empty)
            .OrderBy(variant => variant.Title)
            .ThenBy(variant => variant.Handle)
            .ThenBy(variant => variant.Option1Value)
            .ThenBy(variant => variant.Option2Value)
            .ThenBy(variant => variant.Option3Value)
            .Select(variant => new MissingSkuExportRow
            {
                Handle = variant.Handle,
                Title = variant.Title,
                Option1Value = variant.Option1Value,
                Option2Value = variant.Option2Value,
                Option3Value = variant.Option3Value,
                Sku = variant.Sku,

                LocationCount =
                    variant.InventoryLevels.Count,

                TotalOnHand =
                    variant.InventoryLevels
                        .Sum(level =>
                            (long?)level.OnHandQuantity) ?? 0,

                Issue = "Missing SKU"
            })
            .ToListAsync(cancellationToken);

        return CreateCsvFile(
            rows,
            "catlens-missing-sku");
    }

    [HttpGet]
    public async Task<IActionResult> ExportDuplicateSkus(
        CancellationToken cancellationToken = default)
    {
        var variants = await _dbContext.Variants
            .AsNoTracking()
            .Where(variant =>
                variant.Sku != null &&
                variant.Sku != string.Empty)
            .Select(variant => new
            {
                Sku = variant.Sku!,
                variant.Handle,
                variant.Title,
                variant.Option1Value,
                variant.Option2Value,
                variant.Option3Value,

                LocationCount =
                    variant.InventoryLevels.Count,

                TotalOnHand =
                    variant.InventoryLevels
                        .Sum(level =>
                            (long?)level.OnHandQuantity) ?? 0
            })
            .ToListAsync(cancellationToken);

        var rows = variants
            .GroupBy(
                variant => variant.Sku.Trim(),
                StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .OrderBy(
                group => group.Key,
                StringComparer.OrdinalIgnoreCase)
            .SelectMany(group => group
                .OrderBy(variant => variant.Title)
                .ThenBy(variant => variant.Handle)
                .ThenBy(variant => variant.Option1Value)
                .Select(variant => new DuplicateSkuExportRow
                {
                    Sku = group.Key,
                    DuplicateVariantCount = group.Count(),
                    Handle = variant.Handle,
                    Title = variant.Title,
                    Option1Value = variant.Option1Value,
                    Option2Value = variant.Option2Value,
                    Option3Value = variant.Option3Value,
                    LocationCount = variant.LocationCount,
                    TotalOnHand = variant.TotalOnHand,
                    Issue = "Duplicate SKU"
                }))
            .ToList();

        return CreateCsvFile(
            rows,
            "catlens-duplicate-sku");
    }

    [HttpGet]
    public async Task<IActionResult> ExportHighInventory(
    CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.InventoryLevels
            .AsNoTracking()
            .Where(level =>
                (level.OnHandQuantity ?? 0) >
                _highInventoryThreshold)
            .OrderByDescending(level =>
                level.OnHandQuantity)
            .ThenBy(level => level.Location)
            .ThenBy(level => level.Variant.Title)
            .Select(level => new HighInventoryExportRow
            {
                Handle = level.Variant.Handle,
                Title = level.Variant.Title,
                Option1Value = level.Variant.Option1Value,
                Option2Value = level.Variant.Option2Value,
                Option3Value = level.Variant.Option3Value,
                Sku = level.Variant.Sku,
                Location = level.Location,

                OnHandQuantity =
                    level.OnHandQuantity ?? 0,

                WarningThreshold =
                    _highInventoryThreshold,

                AmountAboveThreshold =
                    (level.OnHandQuantity ?? 0) -
                    _highInventoryThreshold,

                Issue = "High inventory - verify"
            })
            .ToListAsync(cancellationToken);

        return CreateCsvFile(
            rows,
            "catlens-high-inventory");
    }

    private FileContentResult CreateCsvFile<T>(
        IEnumerable<T> rows,
        string filePrefix)
    {
        using var stringWriter =
            new StringWriter(CultureInfo.InvariantCulture);

        using (var csvWriter =
               new CsvWriter(
                   stringWriter,
                   CultureInfo.InvariantCulture))
        {
            csvWriter.WriteRecords(rows);
        }

        var csvBytes = Encoding.UTF8.GetBytes(
            "\uFEFF" + stringWriter.ToString());

        var fileName =
            $"{filePrefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(
            csvBytes,
            "text/csv; charset=utf-8",
            fileName);
    }
}