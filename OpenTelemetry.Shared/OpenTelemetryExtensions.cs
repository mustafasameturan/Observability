using System.Diagnostics;
using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Shared;

public static class OpenTelemetryExtension
{
    public static void AddOpenTelemetryExt(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenTelemetryConstants>(configuration.GetSection("OpenTelemetry"));
        var openTelemetryConstants = (configuration.GetSection("OpenTelemetry").Get<OpenTelemetryConstants>())!;
        
        ActivitySourceProvider.Source = new ActivitySource(openTelemetryConstants.ActivitySourceName);
        
        services.AddOpenTelemetry()
            .WithTracing(options =>
            {
                options
                    .AddSource(openTelemetryConstants?.ActivitySourceName)
                    .AddSource(DiagnosticHeaders.DefaultListenerName)
                    .ConfigureResource(resource =>
                    {
                        resource.AddService(serviceName: openTelemetryConstants?.ServiceName,
                            serviceVersion: openTelemetryConstants?.ServiceVersion);
                    });
                
                options.AddAspNetCoreInstrumentation(aspnetcoreOptions =>
                {
                    // Tüm api başlığı içeren endpointlerdeki verileri endpointleri okur.
                    aspnetcoreOptions.Filter = (context) =>
                    {
                        if (!string.IsNullOrEmpty(context.Request.Path.Value))
                            return context.Request.Path.Value!.Contains("api", StringComparison.InvariantCulture);

                        return false;
                    };

                    // Serilog üzerinden elasticsearch db'ye hatalar gönderildiği için kapatıldı.
                    // Exception fırladığında detaylarını trace olarak tutmak için bu değer true'ya set edilir.
                    // aspnetcoreOptions.RecordException = true;
                    //
                    // aspnetcoreOptions.EnrichWithException = (activity, exception) =>
                    // {
                    //     // Bilerek boş bırakıldı. Örnek için
                    // };
                });
                
                options.AddEntityFrameworkCoreInstrumentation(efcoreOptions =>
                {
                    efcoreOptions.SetDbStatementForText = true;
                    efcoreOptions.SetDbStatementForStoredProcedure = true;
                    
                    // Sql tarafından dönen aktivitleri zengliştirmek için kullanılır
                    efcoreOptions.EnrichWithIDbCommand = (activity, dbCommand) =>
                    {
                        // Bilerek boş bırakıldı. Örnek için
                    };
                });
                
                options.AddHttpClientInstrumentation(httpOptions =>
                {
                    httpOptions.FilterHttpRequestMessage = (request) =>
                    {
                        return !request.RequestUri.AbsoluteUri.Contains("9200", StringComparison.InvariantCulture);
                    };
                    
                    httpOptions.EnrichWithHttpRequestMessage = async (activity, request) =>
                    {
                        var requestContent = "empty";

                        if (request.Content != null)
                        {
                            requestContent = await request.Content.ReadAsStringAsync();
                        }

                        activity.SetTag("http.request.body", requestContent);
                    };

                    httpOptions.EnrichWithHttpResponseMessage = async (activity, response) =>
                    {
                        activity.SetTag("http.response.body", await response.Content.ReadAsStringAsync());
                    }; 
                });

                options.AddRedisInstrumentation(redisOptions =>
                {
                    redisOptions.SetVerboseDatabaseStatements = true;
                });
                
                options.AddConsoleExporter();
                options.AddOtlpExporter(); //Jeager
            });
    }
}