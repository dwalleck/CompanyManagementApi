namespace Employee.Api.Types;

public enum PayEntryType
{
    PayGroup,
    Disbursement
}

public record class PayEntry
{
    public Guid Id { get; set; }
    public PayEntryType Type { get; set; }
    public Guid? PayGroupId { get; set; }
    public Guid? DisbursementId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string RoutingNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    
    // Navigation properties
    public PayGroup? PayGroup { get; set; }
    public Disbursement? Disbursement { get; set; }
    
    // Helper properties for business logic
    public Guid ParentId => Type switch
    {
        PayEntryType.PayGroup => PayGroupId ?? throw new InvalidOperationException("PayGroupId is null"),
        PayEntryType.Disbursement => DisbursementId ?? throw new InvalidOperationException("DisbursementId is null"),
        _ => throw new InvalidOperationException($"Unknown PayEntryType: {Type}")
    };
}
