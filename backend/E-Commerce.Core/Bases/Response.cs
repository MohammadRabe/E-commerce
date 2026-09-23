using System.Net;

namespace E_commerce.Core.Bases;

public sealed record Response<T>(
    HttpStatusCode StatusCode,
    bool IsSuccess,
    T? Data = default,
    string? Message = null,
    IReadOnlyList<string>? Errors = null)
{
    public static Response<T> Success(T? data, string? message = "Success") =>
        new(HttpStatusCode.OK, true, data, message);

    public static Response<T> Created(T? data, string? message = "Created") =>
        new(HttpStatusCode.Created, true, data, message);

    public static Response<T> Failure(
        string error,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) =>
        new(statusCode, false, default, error, new[] { error });
}
