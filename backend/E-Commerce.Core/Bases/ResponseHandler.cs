using System.Net;

namespace E_commerce.Core.Bases;

public class ResponseHandler
{
    public Response<T> Success<T>(T? data, string? message = "Success") =>
        new(HttpStatusCode.OK, true, data, message);

    public Response<T> Created<T>(T? data, string? message = "Created") =>
        new(HttpStatusCode.Created, true, data, message);

    public Response<T> Accepted<T>(T? data, string? message = "Accepted") =>
        new(HttpStatusCode.Accepted, true, data, message);

    public Response<T> BadRequest<T>(
        T? data = default,
        IEnumerable<string>? errors = null,
        string? message = "Bad request") =>
        Create(HttpStatusCode.BadRequest, false, data, message, errors);

    public Response<T> Unauthorized<T>(T? data = default, string? message = "Unauthorized") =>
        Create(HttpStatusCode.Unauthorized, false, data, message);

    public Response<T> Forbidden<T>(T? data = default, string? message = "Forbidden") =>
        Create(HttpStatusCode.Forbidden, false, data, message);

    public Response<T> NotFound<T>(
        T? data = default,
        IEnumerable<string>? errors = null,
        string? message = "Not found") =>
        Create(HttpStatusCode.NotFound, false, data, message, errors);

    public Response<T> Conflict<T>(
        T? data = default,
        IEnumerable<string>? errors = null,
        string? message = "Conflict") =>
        Create(HttpStatusCode.Conflict, false, data, message, errors);

    public Response<T> UnprocessableEntity<T>(
        IEnumerable<string>? errors = null,
        string? message = "Unprocessable entity") =>
        Create<T>(HttpStatusCode.UnprocessableEntity, false, default, message, errors);

    public Response<T> InternalServerError<T>(
        T? data = default,
        IEnumerable<string>? errors = null,
        string? message = "Internal server error") =>
        Create(HttpStatusCode.InternalServerError, false, data, message, errors);

    public Response<T> NoContent<T>(string? message = "No content") =>
        Create<T>(HttpStatusCode.NoContent, true, default, message);

    public Response<T> Deleted<T>(T? data = default, string? message = "Deleted") =>
        Create(HttpStatusCode.NoContent, true, data, message);

    private static Response<T> Create<T>(
        HttpStatusCode statusCode,
        bool isSuccess,
        T? data,
        string? message,
        IEnumerable<string>? errors = null) =>
        new(statusCode, isSuccess, data, message, errors?.ToArray());
}
