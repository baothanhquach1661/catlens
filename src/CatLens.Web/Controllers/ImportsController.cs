using System.Globalization;
using CatLens.Web.Models.Import;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;

namespace CatLens.Web.Controllers;

public class ImportsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(IFormFile? inventoryFile)
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
            await using var stream = inventoryFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(
                reader,
                CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<InventoryCsvRowMap>();

            var rowCount = 0;

            await foreach (
                var row in csv.GetRecordsAsync<InventoryCsvRow>())
            {
                rowCount++;
            }

            TempData["UploadMessage"] =
                $"Successfully read {rowCount:N0} CSV rows " +
                $"from {inventoryFile.FileName}.";
        }
        catch (CsvHelperException)
        {
            ModelState.AddModelError(
                string.Empty,
                "CatLens could not read this CSV. " +
                "Please check that it is a Shopify inventory export.");

            return View();
        }

        return RedirectToAction(nameof(Index));
    }
}