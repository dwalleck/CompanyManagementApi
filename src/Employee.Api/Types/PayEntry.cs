using MassTransit;

namespace Employee.Api.Types;

// Base class for Table per Hierarchy (TPH) pattern
public abstract record PayEntry
{
    public Guid Id { get; set; } = NewId.NextGuid();
    public string EmployeeId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string RoutingNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    // Abstract property to get the parent ID (polymorphic behavior)
    public abstract Guid ParentId { get; }
}

public record PayGroupEntry : PayEntry
{
    public Guid PayGroupId { get; set; }
    public PayGroup PayGroup { get; set; } = null!;

    public override Guid ParentId => PayGroupId;
}

public record DisbursementEntry : PayEntry
{
    public Guid DisbursementId { get; set; }
    public Disbursement Disbursement { get; set; } = null!;

    public override Guid ParentId => DisbursementId;
}
