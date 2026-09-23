using CleanArch.Data.Dtos.Claims_;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Data.Helpers
{
    public static class SystemClaims
    {
        public static readonly List<UserClaimDto> AllClaims = new List<UserClaimDto>()
        {
            new UserClaimDto ("create", "false"),
            new UserClaimDto ("edit", "false"),
            new UserClaimDto ("delete", "false")
        };
        
    }
}
