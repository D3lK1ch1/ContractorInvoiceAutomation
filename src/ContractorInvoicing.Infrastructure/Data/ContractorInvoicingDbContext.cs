using ContractorInvoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContractorInvoicing.Infrastructure.Data;

public class ContractorInvoicingDbContext(DbContextOptions<ContractorInvoicingDbContext> options)
    : DbContext(options)
{
    public DbSet<ContractorProfile> ContractorProfiles => Set<ContractorProfile>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AutomationTask> AutomationTasks => Set<AutomationTask>();
}
