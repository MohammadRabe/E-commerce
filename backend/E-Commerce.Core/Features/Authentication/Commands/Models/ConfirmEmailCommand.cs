using CleanArch.Core.Bases;
using CleanArch.Data.ResultModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authentication.Commands.Models
{
    public class ConfirmEmailCommand : IRequest<Response<ConfirmEmailResultModel>>
    {
        public int UserId { get; set; }
        public string Token { get; set; }
    }
}
