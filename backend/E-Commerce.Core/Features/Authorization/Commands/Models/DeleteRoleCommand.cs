using CleanArch.Core.Bases;
using CleanArch.Data.ResultModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Commands.Models
{
    public class DeleteRoleCommand : IRequest<Response<DeleteRoleResultModel>>
    {
        public int RoleId { get; set; }
    }
}
