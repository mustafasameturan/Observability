using System.Text.Json.Serialization;

namespace Common.Shared.DTOs;

public record ResponseDto<T>
{
    public T Data { get; set; }
    [JsonIgnore]
    public int StatusCode { get; set; }
    public List<String>? Errors { get; set; }
    
    public static ResponseDto<T> Success(int statusCode, T data)
    {
        return new ResponseDto<T> { Data = data, StatusCode = statusCode };
    }
    public static ResponseDto<T> Success(int statusCode)
    {
        return new ResponseDto<T> { StatusCode = statusCode };
    }

    public static ResponseDto<T> Fail(int statusCode, List<string> errors)
    {
        return new ResponseDto<T> { StatusCode = statusCode, Errors = errors };
    }

    public static ResponseDto<T> Fail(int statusCode, string error)
    {
        return new ResponseDto<T> { StatusCode = statusCode, Errors = new List<string> { error } };
    }
}