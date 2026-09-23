using CleanArch.Core.Bases;
using CleanArch.Data.ResultModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authentication.Commands.Models
{
    public class SignInCommand : IRequest<Response<SignInResultModel>>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
