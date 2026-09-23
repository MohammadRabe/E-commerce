using CleanArch.Core.Bases;
using CleanArch.Data.ResultModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Queries.Models
{
    public class ManageUserRoleListQuery : IRequest<Response<ManageRoleListResultModel>>
    {
        public int UserId { get; set; }
    }
}
