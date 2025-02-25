using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Selu383.SP25.P02.Api.Features.Roles;

namespace Selu383.SP25.P02.Api.Data
{
    public static class SeedRoles
    {
        public static async Task Initialize(RoleManager<Role> roleManager)
        {
            // Check if roles exist and create them if they don't
            await ensureRoleExists(roleManager, "Admin");
            await ensureRoleExists(roleManager, "User");
        }

        private static async Task ensureRoleExists(RoleManager<Role> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName)) 
            {
                await roleManager.CreateAsync(new Role { Name = roleName }); 
            }
        }
    }
}

