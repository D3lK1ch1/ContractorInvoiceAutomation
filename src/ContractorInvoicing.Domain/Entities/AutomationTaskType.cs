namespace ContractorInvoicing.Domain.Entities;

public enum AutomationTaskType
{
    ReminderBeforeDue,
    ReminderOnDue,
    OverdueFollowUp,
    SecondOverdueFollowUp
}
