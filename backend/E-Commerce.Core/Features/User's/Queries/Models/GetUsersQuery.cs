using CleanArch.Core.Bases;
using CleanArch.Core.Wrappers;
using CleanArch.Data.Dtos.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.User_s.Queries
{
    public class GetPagedUsersQuery : IRequest<Response<PagedList<GetPagedUsersQueryResponseDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
