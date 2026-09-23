using CleanArch.Core.Bases;
using CleanArch.Data.ResultModels;
using CleanArch.Data.ServiceInputDtos.Roles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Commands.Models
{
    public class EditRoleCommand : EditRoleRequest, IRequest<Response<EditRoleResultModel>>
    {
    }
}
