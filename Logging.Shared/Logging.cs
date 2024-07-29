using Microsoft.AspNetCore.Builder;
using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;

namespace Logging.Shared;

public static class Logging
{
    public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogging =>
        (builderContext, loggerConfiguration) =>
        {
            var environment = builderContext.HostingEnvironment;

            loggerConfiguration
                // Configuration üzerinden yapılan ayarların okunmasını sağlar    
                .ReadFrom.Configuration(builderContext.Configuration)   
                // Ortak bir yerden trace id üretmek için
                .Enrich.FromLogContext()
                // Bir hata fırlar ise detaylarını da yazması için
                .Enrich.WithExceptionDetails()
                // Hangi ortamdan atıldığı
                .Enrich.WithProperty("Env", environment.EnvironmentName)
                // Hangi uygulamadan atıldığı
                .Enrich.WithProperty("AppName", environment.ApplicationName);

            var elasticSearchBaseUrl = builderContext.Configuration.GetSection("Elasticsearch")["BaseUrl"];
            var userName = builderContext.Configuration.GetSection("Elasticsearch")["UserName"];
            var password = builderContext.Configuration.GetSection("Elasticsearch")["Password"];
            var indexName = builderContext.Configuration.GetSection("Elasticsearch")["IndexName"];

            loggerConfiguration.WriteTo.Elasticsearch(new(new Uri(elasticSearchBaseUrl!))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
                IndexFormat = $"{indexName}-{environment.EnvironmentName}-logs-" + "{0:yyy.MM.dd}",
                ModifyConnectionSettings = x => x.BasicAuthentication(userName, password),
                CustomFormatter = new ElasticsearchJsonFormatter()
            });
        };

    public static void AddOpenTelemetryLog(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(cfg =>
        {
            var serviceName = builder.Configuration.GetSection("OpenTelemetry")["ServiceName"];
            var serviceVersion = builder.Configuration.GetSection("OpenTelemetry")["ServiceVersion"]; 
            
            cfg.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName!, 
                serviceVersion: serviceVersion));

            cfg.AddOtlpExporter();
        });
    }
}