using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Data.Helpers.Users
{
    public class GetPagedUsersQueryResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? Contry { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
