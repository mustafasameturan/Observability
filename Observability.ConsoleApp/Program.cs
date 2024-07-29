using System.Diagnostics;
using Observability.ConsoleApp;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

/*
    * Bu kısımda activity(span)'ler burada listener ile dinlenebilir.
    * Bu senaryoda activityler istenilen her yere yazılabilir. (Ef Core, SQL server etc.)
*/
ActivitySource.AddActivityListener(new ActivityListener()
{
    ShouldListenTo = source => source.Name == OpenTelemetryConstants.ActivitySourceFileName,
    ActivityStarted = activity => {
        Console.WriteLine("Activity started!");
    },
    ActivityStopped = activity =>
    {
        Console.WriteLine("Activity stopped!");
    }
});

using var traceProviderFile = Sdk.CreateTracerProviderBuilder()
    .AddSource(OpenTelemetryConstants.ActivitySourceFileName)
    .Build();

using var traceProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(OpenTelemetryConstants.ActiviySourceName)
    .ConfigureResource(configure =>
    {
        configure
            .AddService(OpenTelemetryConstants.ServiceName, serviceVersion: OpenTelemetryConstants.ServiceVersion)
            .AddAttributes(new List<KeyValuePair<string, object>>()
            {
                new("host.machineName", Environment.MachineName),
                new("host.environment", "dev")
            });
    })
    .AddConsoleExporter()
    .AddOtlpExporter()
    .AddZipkinExporter(zipkinOptions =>
    {
        zipkinOptions.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
    })
    .Build();

ServiceHelper serviceHelper = new ServiceHelper();
await serviceHelper.Work1();
serviceHelper.Work2();