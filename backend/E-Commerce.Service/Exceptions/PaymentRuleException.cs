using System.Net;

namespace E_commerce.Service.Exceptions;

public sealed class PaymentRuleException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
