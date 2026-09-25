using CatLens.Web.Data;
using CatLens.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatLens.Web.Controllers;

public class CatalogController : Controller
{
    private readonly CatLensDbContext _dbContext;

    public CatalogController(CatLensDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var variants = await _dbContext.Variants
            .AsNoTracking()
            .OrderBy(variant => variant.Title)
            .ThenBy(variant => variant.Handle)
            .ThenBy(variant => variant.Option1Value)
            .Take(50)
            .Select(variant => new CatalogVariantRowViewModel
            {
                Handle = variant.Handle,
                Title = variant.Title,
                Option1Value = variant.Option1Value,
                Option2Value = variant.Option2Value,
                Option3Value = variant.Option3Value,
                Sku = variant.Sku,

                LocationCount =
                    variant.InventoryLevels.Count,

                StockedLocationCount =
                    variant.InventoryLevels.Count(
                        level => level.OnHandQuantity != null),

                TotalOnHand =
                    variant.InventoryLevels
                        .Sum(level =>
                            (long?)level.OnHandQuantity) ?? 0
            })
            .ToListAsync(cancellationToken);

        var model = new CatalogIndexViewModel
        {
            TotalVariants =
                await _dbContext.Variants.CountAsync(
                    cancellationToken),

            TotalInventoryLevels =
                await _dbContext.InventoryLevels.CountAsync(
                    cancellationToken),

            MissingSkuVariants =
                await _dbContext.Variants.CountAsync(
                    variant =>
                        variant.Sku == null ||
                        variant.Sku == string.Empty,
                    cancellationToken),

            Variants = variants
        };

        return View(model);
    }
}