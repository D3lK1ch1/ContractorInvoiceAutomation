namespace ContractorInvoicing.Domain.Entities;

public class Client
{
    public int Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string DisplayName => string.IsNullOrWhiteSpace(BusinessName) ? ContactName : BusinessName;
    public string Email { get; set; } = string.Empty;
    public string? Abn { get; set; }
    public int? DefaultPaymentTermsDays { get; set; }
    public decimal? DefaultHourlyRate { get; set; }
    public int NextInvoiceSequence { get; set; } = 1;
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Invoice> Invoices { get; set; } = [];
}
