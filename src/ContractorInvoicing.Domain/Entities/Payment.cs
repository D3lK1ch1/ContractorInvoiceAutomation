namespace ContractorInvoicing.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public DateOnly PaidAt { get; set; }
    public decimal Amount { get; set; }
    public string? Method { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
