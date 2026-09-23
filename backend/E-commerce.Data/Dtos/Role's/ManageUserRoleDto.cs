using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Data.Dtos.Role_s
{
    public class ManageUserRoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool HasRole { get; set; }
    }
}
