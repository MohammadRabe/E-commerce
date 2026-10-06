using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Service.Abstraction
{
    public interface IPaymentService
    {
        Task<E_commerce.Data.Dtos.Payment.Fawaterak.FawaterakTransactionDataResultModel?> CreatePaymentAsync(int orderId, string userId, CancellationToken cancellationToken = default);
        Task<bool?> VerifyPaymentAsync(int orderId, string userId, CancellationToken cancellationToken = default);
    }
}
