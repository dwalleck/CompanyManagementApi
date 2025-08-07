using MassTransit;

namespace Employee.Api.Types;

public record class PayGroup
{
    public Guid Id { get; set; } = NewId.NextGuid();
    public PayType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<PayGroupEntry> PayEntries { get; set; } = null!;
    public ICollection<Disbursement> Disbursements { get; set; } = null!;
    public ICollection<string> Approvers { get; set; } = null!;
}
