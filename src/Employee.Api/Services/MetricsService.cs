using System.Diagnostics.Metrics;

namespace Employee.Api.Services;

public class MetricsService
{
    private readonly Meter _meter;
    private readonly Counter<long> _payGroupCreated;
    private readonly Counter<long> _disbursementCreated;
    private readonly Counter<long> _payEntryCreated;
    private readonly Histogram<double> _queryDuration;
    private readonly Histogram<double> _mutationDuration;
    private readonly Counter<long> _validationFailures;
    private readonly UpDownCounter<int> _activeDisbursements;

    public MetricsService()
    {
        _meter = new Meter("Employee.Api.Metrics", "1.0.0");
        
        // Business operation counters
        _payGroupCreated = _meter.CreateCounter<long>(
            "paygroup_created_total",
            description: "Total number of PayGroups created");
            
        _disbursementCreated = _meter.CreateCounter<long>(
            "disbursement_created_total", 
            description: "Total number of Disbursements created");
            
        _payEntryCreated = _meter.CreateCounter<long>(
            "payentry_created_total",
            description: "Total number of PayEntries created");

        // Performance metrics
        _queryDuration = _meter.CreateHistogram<double>(
            "graphql_query_duration_seconds",
            unit: "s",
            description: "Duration of GraphQL queries in seconds");
            
        _mutationDuration = _meter.CreateHistogram<double>(
            "graphql_mutation_duration_seconds", 
            unit: "s",
            description: "Duration of GraphQL mutations in seconds");

        // Quality metrics
        _validationFailures = _meter.CreateCounter<long>(
            "validation_failures_total",
            description: "Total number of validation failures");

        // State metrics
        _activeDisbursements = _meter.CreateUpDownCounter<int>(
            "active_disbursements",
            description: "Number of active disbursements in the system");
    }

    // Business metrics
    public void RecordPayGroupCreated(string payType) 
        => _payGroupCreated.Add(1, new KeyValuePair<string, object?>("pay_type", payType));

    public void RecordDisbursementCreated(string state)
        => _disbursementCreated.Add(1, new KeyValuePair<string, object?>("initial_state", state));
        
    public void RecordPayEntryCreated(string entryType)
        => _payEntryCreated.Add(1, new KeyValuePair<string, object?>("entry_type", entryType));

    // Performance metrics  
    public void RecordQueryDuration(string queryName, double durationSeconds)
        => _queryDuration.Record(durationSeconds, new KeyValuePair<string, object?>("query_name", queryName));
        
    public void RecordMutationDuration(string mutationName, double durationSeconds)
        => _mutationDuration.Record(durationSeconds, new KeyValuePair<string, object?>("mutation_name", mutationName));

    // Quality metrics
    public void RecordValidationFailure(string entityType, string validationType)
        => _validationFailures.Add(1, 
            new KeyValuePair<string, object?>("entity_type", entityType),
            new KeyValuePair<string, object?>("validation_type", validationType));

    // State tracking
    public void IncrementActiveDisbursements() => _activeDisbursements.Add(1);
    public void DecrementActiveDisbursements() => _activeDisbursements.Add(-1);

    public void Dispose()
    {
        _meter?.Dispose();
    }
}