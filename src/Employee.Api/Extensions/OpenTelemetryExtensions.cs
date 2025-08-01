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
                    .AddRuntimeInstrumentation();
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
            {
                ["service.namespace"] = "CompanyManagement",
                ["deployment.environment"] = builder.Environment.EnvironmentName
            });
    }
}