using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.IO;

namespace Common.Shared;

public class RequestAndResponseActivityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    
    public RequestAndResponseActivityMiddleware(RequestDelegate next)
    {
        _next = next;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await AddRequestBodyContentToActivityTags(context);
        await AddResponseBodyContentToActivityTags(context);
    }
    
    /// <summary>
    /// Request body content'i activity tag olarak ekler
    /// </summary>
    /// <param name="context"></param>
    private async Task AddRequestBodyContentToActivityTags(HttpContext context)
    {
        context.Request.EnableBuffering();
        var requestBodyStreamReader = new StreamReader(context.Request.Body);
        var requestBodyContent = await requestBodyStreamReader.ReadToEndAsync();
        Activity.Current?.SetTag("http.request.body", requestBodyContent);
        context.Request.Body.Position = 0;
    }
    
    /// <summary>
    /// Response body content'i activity tag olarak ekler.
    /// </summary>
    /// <param name="context"></param>
    private async Task AddResponseBodyContentToActivityTags(HttpContext context)
    {
        var originalResponse = context.Response.Body;
        await using var responseBodyMemoryStream = _recyclableMemoryStreamManager.GetStream();
        context.Response.Body = responseBodyMemoryStream;

        await _next(context);
        
        // Binary formattan okuduğu için 0'a aldık
        responseBodyMemoryStream.Position = 0;

        var responseBodyStreamReader = new StreamReader(responseBodyMemoryStream);
        var responseBodyContent = await responseBodyStreamReader.ReadToEndAsync();
        Activity.Current?.SetTag("http.response.body", responseBodyContent);

        responseBodyMemoryStream.Position = 0;
        await responseBodyMemoryStream.CopyToAsync(originalResponse);
    }
}