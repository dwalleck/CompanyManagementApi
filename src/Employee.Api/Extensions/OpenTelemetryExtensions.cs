using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry;

namespace Employee.Api.Extensions;

public static class OpenTelemetryExtensions
{
    public static IHostApplicationBuilder AddOpenTelemetryConfiguration(this IHostApplicationBuilder builder)
    {
        // Configure OpenTelemetry
        var otelBuilder = builder.Services
            .AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(CreateResourceBuilder(builder))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    // Entity Framework Core and database metrics are auto-collected
                    .AddView("*", MetricStreamConfiguration.Drop) // Start with nothing, then add what we want
                    .AddView("http.server.request.duration", new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries = [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
                    })
                    .AddMeter("Microsoft.EntityFrameworkCore")
                    .AddMeter("Npgsql")
                    .AddMeter("Employee.Api.Metrics");
            })
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(CreateResourceBuilder(builder))
                    .AddSource(builder.Environment.ApplicationName)
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Exclude health check requests from tracing
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments("/health") &&
                            !context.Request.Path.StartsWithSegments("/alive");
                    })
                    .AddHotChocolateInstrumentation()
                    .AddAWSInstrumentation()
                    .AddAWSLambdaConfigurations();
            });

        // Add OTLP exporter if endpoint is configured
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            otelBuilder.UseOtlpExporter();
        }

        // Configure OpenTelemetry Logging
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.SetResourceBuilder(CreateResourceBuilder(builder));
        });

        return builder;
    }

    private static ResourceBuilder CreateResourceBuilder(IHostApplicationBuilder builder)
    {
        return ResourceBuilder.CreateDefault()
            .AddService("Employee.Api")
            .AddAttributes(new Dictionary<string, object>
(StringComparer.OrdinalIgnoreCase)
            {
                ["service.namespace"] = "CompanyManagement",
                ["deployment.environment"] = builder.Environment.EnvironmentName,
            });
    }
}