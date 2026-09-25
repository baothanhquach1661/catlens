using CatLens.Web.Services;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatLens.Web.Controllers;

public class ImportsController : Controller
{
    private readonly InventoryImportService _importService;

    public ImportsController(
        InventoryImportService importService)
    {
        _importService = importService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        IFormFile? inventoryFile,
        CancellationToken cancellationToken)
    {
        if (inventoryFile is null || inventoryFile.Length == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "Please select a CSV file.");

            return View();
        }

        if (!string.Equals(
                Path.GetExtension(inventoryFile.FileName),
                ".csv",
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                string.Empty,
                "The selected file must be a CSV file.");

            return View();
        }

        try
        {
            await using var stream =
                inventoryFile.OpenReadStream();

            var result = await _importService.ImportAsync(
                stream,
                cancellationToken);

            TempData["UploadMessage"] =
                $"Import completed: " +
                $"{result.CsvRowCount:N0} CSV rows, " +
                $"{result.VariantCount:N0} variants, " +
                $"{result.InventoryLevelCount:N0} inventory levels, " +
                $"{result.NotStockedCount:N0} not stocked.";
        }
        catch (CsvHelperException)
        {
            ModelState.AddModelError(
                string.Empty,
                "CatLens could not read this CSV. " +
                "Please check that it is a Shopify inventory export.");

            return View();
        }
        catch (InvalidDataException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            return View();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The CSV was valid, but CatLens could not save " +
                "the inventory to the database.");

            return View();
        }

        return RedirectToAction(nameof(Index));
    }
}