namespace ContractorInvoicing.Domain.Entities;

public class InvoiceLineItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsTaxable { get; set; } = true;
    public decimal LineSubtotal { get; set; }
    public decimal GstAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }
}
