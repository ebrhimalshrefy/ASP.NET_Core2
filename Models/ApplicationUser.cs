using Microsoft.AspNetCore.Identity;

namespace Week5Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Custom profile properties if needed in future
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
