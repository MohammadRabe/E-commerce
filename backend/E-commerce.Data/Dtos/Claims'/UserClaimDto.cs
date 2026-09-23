using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace CleanArch.Data.Dtos.Claims_
{
    public class UserClaimDto : Claim
    {
        public UserClaimDto(string type, string value) : base(type,value)
        {
           
        }

    }
}
