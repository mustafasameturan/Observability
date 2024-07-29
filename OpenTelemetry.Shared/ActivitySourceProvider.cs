using System.Diagnostics;

namespace OpenTelemetry.Shared;

/*
    * Activity resource trace data'yı üreteb ana kaynaktır.
    * Uygulama boyunca static veya singleton olabilir. 
*/
public static class ActivitySourceProvider
{
    public static ActivitySource Source = null!;
}