using CleanArch.Core.Bases;
using CleanArch.Data.Dtos.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.User_s.Queries.Models
{
    public class GetUserByIdQuery : IRequest<Response<GetUserByIdDto>>
    {
        public int Id { get; set; }
    }
}
