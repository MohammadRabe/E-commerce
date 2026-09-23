using CleanArch.Core.Bases;
using CleanArch.Data.ResultModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Commands.Models
{
    
    public class AddUserToRoleCommand : IRequest<Response<EditUserRoleResultModel>>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
