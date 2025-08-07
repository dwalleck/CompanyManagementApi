using Employee.Api.Types;
using OneOf;

namespace Employee.Api.Extensions;

public static class PayEntryExtensions
{
    // Get parent using TPH pattern - now uses polymorphism instead of switching
    public static OneOf<PayGroup, Disbursement> GetParent(this PayEntry payEntry)
    {
        return payEntry switch
        {
            PayGroupEntry pgEntry => pgEntry.PayGroup ?? throw new InvalidOperationException("PayGroup is null"),
            DisbursementEntry dEntry => dEntry.Disbursement ?? throw new InvalidOperationException("Disbursement is null"),
            _ => throw new InvalidOperationException($"Unknown PayEntry type: {payEntry.GetType().Name}")
        };
    }
    
    // Factory method for creating PayGroupEntry
    public static PayGroupEntry CreateForPayGroup(Guid payGroupId, string employeeId, string accountNumber, string routingNumber, decimal amount)
    {
        return new PayGroupEntry
        {
            Id = Guid.NewGuid(),
            PayGroupId = payGroupId,
            EmployeeId = employeeId,
            AccountNumber = accountNumber,
            RoutingNumber = routingNumber,
            Amount = amount
        };
    }
    
    // Factory method for creating DisbursementEntry
    public static DisbursementEntry CreateForDisbursement(Guid disbursementId, string employeeId, string accountNumber, string routingNumber, decimal amount)
    {
        return new DisbursementEntry
        {
            Id = Guid.NewGuid(),
            DisbursementId = disbursementId,
            EmployeeId = employeeId,
            AccountNumber = accountNumber,
            RoutingNumber = routingNumber,
            Amount = amount
        };
    }
}