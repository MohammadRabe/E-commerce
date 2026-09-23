using CleanArch.Core.Bases;
using CleanArch.Data.Dtos.Claims_;
using CleanArch.Data.ResultModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Commands.Models
{
    public class UpdateUserClaimsCommand : IRequest<Response<ManageUserClaimsResultModel>>
    {
        public int UserId { get; set; }
        public List<UserClaimDto> Claims { get; set; } = new List<UserClaimDto>();
    }
}
