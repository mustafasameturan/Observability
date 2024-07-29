using System.Diagnostics;

namespace Observability.ConsoleApp;

/*
    * Activity resource trace data'yı üreteb ana kaynaktır.
    * Uygulama boyunca static veya singleton olabilir. 
*/
internal static class ActivitySourceProvider
{
    public static ActivitySource Source = new ActivitySource(OpenTelemetryConstants.ActiviySourceName);
    public static ActivitySource SourceFile = new ActivitySource(OpenTelemetryConstants.ActivitySourceFileName);
}