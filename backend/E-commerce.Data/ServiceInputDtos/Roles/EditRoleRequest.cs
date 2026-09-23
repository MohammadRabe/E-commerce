using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Data.ServiceInputDtos.Roles
{
    public class EditRoleRequest
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
