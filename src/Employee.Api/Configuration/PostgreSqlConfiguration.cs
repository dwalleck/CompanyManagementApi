namespace Employee.Api.Configuration;

public class PostgreSqlConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    
    // Connection string builder options
    public int MinPoolSize { get; set; } = 5;
    public int MaxPoolSize { get; set; } = 100;
    public int ConnectionIdleLifetime { get; set; } = 300;
    public int Timeout { get; set; } = 15;
    public int KeepAlive { get; set; } = 30;
    public string ApplicationName { get; set; } = "EmployeeApi";
    public bool TcpKeepAlive { get; set; } = true;
    public bool Enlist { get; set; } = true;
}