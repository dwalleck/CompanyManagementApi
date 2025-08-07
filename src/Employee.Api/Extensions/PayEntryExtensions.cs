using Employee.Api.Types;
using OneOf;

namespace Employee.Api.Extensions;

public static class PayEntryExtensions
{
    public static OneOf<PayGroup, Disbursement> GetParent(this PayEntry payEntry)
    {
        return payEntry.Type switch
        {
            PayEntryType.PayGroup => payEntry.PayGroup ?? throw new InvalidOperationException("PayGroup is null"),
            PayEntryType.Disbursement => payEntry.Disbursement ?? throw new InvalidOperationException("Disbursement is null"),
            _ => throw new InvalidOperationException($"Unknown PayEntryType: {payEntry.Type}")
        };
    }
    
    public static PayEntry CreateForPayGroup(Guid payGroupId, string employeeId, string accountNumber, string routingNumber, decimal amount)
    {
        return new PayEntry
        {
            Id = Guid.NewGuid(),
            Type = PayEntryType.PayGroup,
            PayGroupId = payGroupId,
            DisbursementId = null,
            EmployeeId = employeeId,
            AccountNumber = accountNumber,
            RoutingNumber = routingNumber,
            Amount = amount
        };
    }
    
    public static PayEntry CreateForDisbursement(Guid disbursementId, string employeeId, string accountNumber, string routingNumber, decimal amount)
    {
        return new PayEntry
        {
            Id = Guid.NewGuid(),
            Type = PayEntryType.Disbursement,
            PayGroupId = null,
            DisbursementId = disbursementId,
            EmployeeId = employeeId,
            AccountNumber = accountNumber,
            RoutingNumber = routingNumber,
            Amount = amount
        };
    }
}