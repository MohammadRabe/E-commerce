using Microsoft.AspNetCore.Identity;

namespace E_commerce.Data.Entities
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<UserNotification> Notifications { get; set; } = new List<UserNotification>();
    }
}
