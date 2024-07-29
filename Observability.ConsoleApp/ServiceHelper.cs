using System.Diagnostics;

namespace Observability.ConsoleApp;

internal class ServiceHelper
{
    internal async Task Work1()
    {
        using var activity = ActivitySourceProvider.Source.StartActivity();
        var serviceOne = new ServiceOne();

        Console.WriteLine($"google response length: {await serviceOne.MakeRequestToGoogle()}");   
        Console.WriteLine("Work1 completed!");   
    }

    internal void Work2()
    {
        using var activity = ActivitySourceProvider.SourceFile.StartActivity();
        activity.SetTag("work 2 tag", "work 2 tag value");
        activity.AddEvent(new ActivityEvent("work 2 event"));
    }
}