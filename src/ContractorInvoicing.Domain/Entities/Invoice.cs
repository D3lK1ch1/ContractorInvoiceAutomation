namespace ContractorInvoicing.Domain.Entities;

public class Invoice
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public decimal Subtotal { get; set; }
    public decimal GstAmount { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<InvoiceLineItem> LineItems { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
    public List<AutomationTask> AutomationTasks { get; set; } = [];
}
