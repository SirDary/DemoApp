using Microsoft.AspNetCore.Identity;

namespace Demo_Auth.Data.Models
{
    public class AppUser : IdentityUser
    {
        public virtual Employee Employee { get; set; } = new();
    }
}
