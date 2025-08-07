using MassTransit;

namespace Employee.Api.Types;

public record class BusinessEmployee
{
    public Guid Id { get; set; } = NewId.NextGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<BankAccount> BankAccounts { get; set; } = [];
    
    public bool IsValid => Math.Abs(BankAccounts.Sum(ba => ba.PayPercentage) - 1.0m) < 0.001m;
}

public record class BankAccount
{
    public string AccountId { get; set; } = string.Empty;
    public string RoutingNumber { get; set; } = string.Empty;
    public decimal PayPercentage { get; set; }
}