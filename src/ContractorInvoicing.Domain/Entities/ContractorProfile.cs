namespace ContractorInvoicing.Domain.Entities;

public class ContractorProfile
{
    public int Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Abn { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PaymentDetails { get; set; }
    public bool IsGstRegistered { get; set; }
    public int DefaultPaymentTermsDays { get; set; } = 14;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
