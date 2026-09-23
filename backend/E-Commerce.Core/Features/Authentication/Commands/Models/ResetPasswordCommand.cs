using CleanArch.Core.Bases;
using CleanArch.Data.ResultModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authentication.Commands.Models
{
    public class ResetPasswordCommand : IRequest<Response<ResetPasswordResultModel>>
    {
        public string Email { get; set; }
    }
}
