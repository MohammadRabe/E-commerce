using CleanArch.Core.Bases;
using CleanArch.Data.Dtos.Role_s;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Queries.Models
{
    public class RolesListQuery : IRequest<Response<List<GetRolesListDto>>>
    {
    }
}
