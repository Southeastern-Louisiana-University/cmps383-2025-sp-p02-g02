using Microsoft.AspNetCore.Identity;
using Selu383.SP25.P02.Api.Features.Users;
using Selu383.SP25.P02.Api.Features.Roles;

namespace Selu383.SP25.P02.Api.Features.UserRoles
{
    public class UserRole : IdentityUserRole<int>
    {
        public required Role Role { get; set; } // many to many through UserRole 
        public required User User { get; set; }  // many to many through UserRole
    }
}
