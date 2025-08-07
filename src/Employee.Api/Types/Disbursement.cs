namespace Employee.Api.Types;

public record class Disbursement
{
    public Guid Id { get; set; }
    public Guid PayGroupId { get; set; }
    public DateTimeOffset DisbursementDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DisbursementState State { get; set; }    
    public PayGroup PayGroup { get; set; } = null!;
    public ICollection<PayEntry> PayEntries { get; set; } = null!;
}
