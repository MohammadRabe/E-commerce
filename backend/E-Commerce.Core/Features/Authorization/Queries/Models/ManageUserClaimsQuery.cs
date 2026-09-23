using CleanArch.Core.Bases;
using CleanArch.Data.ResultModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Queries.Models
{
    public class ManageUserClaimsQuery : IRequest<Response<ManageUserClaimsResultModel>>
    {
        public int UserId { get; set; }
    }
}
