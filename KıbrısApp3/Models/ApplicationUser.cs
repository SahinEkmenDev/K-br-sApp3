using Microsoft.AspNetCore.Identity;

namespace KıbrısApp3.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? ProfileImageUrl { get; set; }

    }
}
