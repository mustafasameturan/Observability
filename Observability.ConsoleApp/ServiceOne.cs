using System.Diagnostics;

namespace Observability.ConsoleApp;

internal class ServiceOne
{
    static HttpClient httpClient = new HttpClient();
    
    internal async Task<int> MakeRequestToGoogle()
    {
        using var activity = 
            ActivitySourceProvider.Source.StartActivity(ActivityKind.Producer, name: "CustomMakeRequestToGoogle");

        try
        {
            var eventTags = new ActivityTagsCollection();

            activity?.AddEvent(new("Google'a istek başladı", tags: eventTags));
            activity?.AddTag("request.schema", "https");
            activity?.AddTag("request.method", "get");

            var result = await httpClient.GetAsync("https://www.google.com");
            var responseContent = await result.Content.ReadAsStringAsync();

            activity?.AddTag("response.length", responseContent.Length);
            
            eventTags.Add("googleBodyLength", responseContent.Length);
            activity?.AddEvent(new("Google'a istek tamamlandı", tags: eventTags));
            
            var serviceTwo = new ServiceTwo();
            var fileLength = await serviceTwo.WriteToFile("Hello World");
            
            return responseContent.Length;
        }
        catch (Exception e)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "İstek yaparken hata meydana geldi!");
            return -1;
        }
    }
}