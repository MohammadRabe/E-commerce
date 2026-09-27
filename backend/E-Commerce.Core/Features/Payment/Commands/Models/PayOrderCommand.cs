using E_commerce.Core.Bases;
using E_commerce.Data.Dtos.Payment.Fawaterak;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Core.Features.Payment.Commands.Models
{

    public sealed record ParOrderCommand
    : IRequest<Response<FawaterakTransactionDataResultModel>>
    {
        public int OrderId { get; set; }
    }




}
