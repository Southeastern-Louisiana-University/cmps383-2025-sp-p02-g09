using Microsoft.AspNetCore.Identity;

namespace Selu383.SP25.P02.Api.Features.Users
{
    public class Role : IdentityRole<int>
    {
        public virtual ICollection<UserRole> Users { get; set; } = new List<UserRole>();
    }
}