using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Features.Roles;
using Selu383.SP25.P02.Api.Features.Theaters;
using Selu383.SP25.P02.Api.Features.UserRoles;
using Selu383.SP25.P02.Api.Features.Users;

namespace Selu383.SP25.P02.Api.Data
{
    public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Theater> Theaters { get; set; }
        
    }
}
