using System.Diagnostics;

namespace Observability.ConsoleApp;

internal class ServiceTwo
{
    internal async Task<int> WriteToFile(string text)
    {
        Activity.Current?.SetTag("güncel activity", "1");

        using var activity = ActivitySourceProvider.Source.StartActivity("a");
        
        await File.WriteAllTextAsync("myFile.txt", text);

        return (await File.ReadAllTextAsync("myFile.txt")).Length;   
    }
}