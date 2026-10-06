using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Service.Abstraction
{
    public interface IPaymentService
    {
        Task<E_commerce.Data.Dtos.Payment.Fawaterak.FawaterakTransactionDataResultModel?> CreatePaymentAsync(int orderId, string userId, CancellationToken cancellationToken = default);
        Task<PaymentVerificationResult?> VerifyPaymentAsync(int orderId, string userId, CancellationToken cancellationToken = default);
    }

    public sealed record PaymentVerificationResult(string Outcome, int? ProviderPaid, decimal? ProviderTotal, string? ProviderCurrency, string? Reason);
}
