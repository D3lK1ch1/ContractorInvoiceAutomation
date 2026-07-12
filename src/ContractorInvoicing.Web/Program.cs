using System.Text;
using ContractorInvoicing.Domain.Services;
using ContractorInvoicing.Infrastructure.Data;
using ContractorInvoicing.Infrastructure.Services;
using ContractorInvoicing.Web.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<ContractorInvoicingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IInvoiceCalculationService, InvoiceCalculationService>();
builder.Services.AddScoped<IInvoiceNumberService, InvoiceNumberService>();
builder.Services.AddScoped<IInvoiceStatusService, InvoiceStatusService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceCsvService, InvoiceCsvService>();
builder.Services.AddScoped<IInvoiceXlsxService, InvoiceXlsxService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/export/invoices.csv", async (IInvoiceCsvService csvService) =>
{
    var csv = await csvService.ExportAllAsync();
    return Results.File(Encoding.UTF8.GetBytes(csv), "text/csv", "invoices.csv");
});

app.MapGet("/export/invoices/{id:int}.csv", async (int id, IInvoiceCsvService csvService) =>
{
    var export = await csvService.ExportInvoiceAsync(id);
    if (export is null)
        return Results.NotFound();

    var invalidChars = Path.GetInvalidFileNameChars();
    var safeName = new string(export.InvoiceNumber.Select(c => invalidChars.Contains(c) ? '-' : c).ToArray());

    return Results.File(Encoding.UTF8.GetBytes(export.Csv), "text/csv", $"{safeName}.csv");
});

app.MapGet("/export/invoices/{id:int}.xlsx", async (int id, IInvoiceXlsxService xlsxService) =>
{
    var export = await xlsxService.ExportInvoiceAsync(id);
    if (export is null)
        return Results.NotFound();

    var invalidChars = Path.GetInvalidFileNameChars();
    var safeName = new string(export.InvoiceNumber.Select(c => invalidChars.Contains(c) ? '-' : c).ToArray());

    return Results.File(export.Content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{safeName}.xlsx");
});

app.Run();
