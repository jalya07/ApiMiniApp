namespace WebApplication2.Helper;

public class ResponseModelHelper<T>
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public T? Data { get; set; }
    public int StatusCode { get; set; }

    public static ResponseModelHelper<T> SuccessResult(T data) => new()
    {
        Success = true,
        Errors = new List<string>(),
        Data = data,
        StatusCode = 200
    };

    public static ResponseModelHelper<T> CreatedResult(T data) => new()
    {
        Success = true,
        Errors = new List<string>(),
        Data = data,
        StatusCode = 201
    };

    public static ResponseModelHelper<T> BadRequestResult(params string[] errors) => new()
    {
        Success = false,
        Errors = new List<string>(errors),
        Data = default,
        StatusCode = 400
    };

    public static ResponseModelHelper<T> UnauthorizedResult(params string[] errors) => new()
    {
        Success = false,
        Errors = errors.Length > 0 ? new List<string>(errors) : new List<string> { "Unauthorized" },
        Data = default,
        StatusCode = 401
    };

    public static ResponseModelHelper<T> NotFoundResult(params string[] errors) => new()
    {
        Success = false,
        Errors = errors.Length > 0 ? new List<string>(errors) : new List<string> { "Not found" },
        Data = default,
        StatusCode = 404
    };

    public static ResponseModelHelper<T> ConflictResult(params string[] errors) => new()
    {
        Success = false,
        Errors = errors.Length > 0 ? new List<string>(errors) : new List<string> { "Conflict" },
        Data = default,
        StatusCode = 409
    };

    public static ResponseModelHelper<T> ErrorResult(params string[] errors) => new()
    {
        Success = false,
        Errors = new List<string>(errors),
        Data = default,
        StatusCode = 500
    };
}

public static class ResponseModelHelper
{
    public static ResponseModelHelper<string> CreateErrorResponse(string message) =>
        ResponseModelHelper<string>.ErrorResult(message);

    public static ResponseModelHelper<string> CreateErrorResponse(List<string> messages) =>
        ResponseModelHelper<string>.ErrorResult(messages.ToArray());
}