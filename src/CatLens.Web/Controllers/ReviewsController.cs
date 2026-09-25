using CatLens.Web.Data;
using CatLens.Web.Models.Reviews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatLens.Web.Controllers;

public class ReviewsController : Controller
{
    private const int PageSize = 50;

    private readonly CatLensDbContext _dbContext;

    public ReviewsController(CatLensDbContext dbContext)
    {
        _dbContext = dbContext;
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
            await missingSkuQuery.CountAsync(
                cancellationToken);

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
                    variant.Sku.ToUpper()))
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
}