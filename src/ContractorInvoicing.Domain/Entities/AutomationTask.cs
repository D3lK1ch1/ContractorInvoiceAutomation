namespace ContractorInvoicing.Domain.Entities;

public class AutomationTask
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public AutomationTaskType TaskType { get; set; }
    public DateOnly ScheduledFor { get; set; }
    public AutomationTaskStatus Status { get; set; } = AutomationTaskStatus.Pending;
    public string? DraftMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
