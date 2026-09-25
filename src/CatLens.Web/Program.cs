using CatLens.Web.Data;
using Microsoft.EntityFrameworkCore;
using CatLens.Web.Services;
using CatLens.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("CatLensDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'CatLensDatabase' was not found.");

builder.Services.AddDbContext<CatLensDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<InventoryImportService>();

builder.Services
    .AddOptions<CatalogReviewOptions>()
    .Bind(
        builder.Configuration.GetSection(
            CatalogReviewOptions.SectionName))
    .Validate(
        options => options.HighInventoryThreshold >= 0,
        "High inventory threshold cannot be negative.")
    .ValidateOnStart();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
