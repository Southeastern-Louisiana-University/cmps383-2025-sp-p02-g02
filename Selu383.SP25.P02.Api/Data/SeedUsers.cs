using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Features.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Selu383.SP25.P02.Api.Data
{
    public static class SeedUsers
    {
        public static async Task Initialize(UserManager<User> userManager)
        {
            var usersExist = await userManager.Users.AnyAsync(); 
            if (usersExist)
            {
                return; 
            }

            await ensureUserExists(userManager, "galkadi", "Password123!", "Admin");
            await ensureUserExists(userManager, "bob", "Password123!", "User");
            await ensureUserExists(userManager, "sue", "Password123!", "User");
        }

        private static async Task ensureUserExists(UserManager<User> userManager, string username, string password, string role)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user != null)
            {
                return; 
            }

            user = new User { UserName = username };
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        public static async Task ResetAndSeedUsers(UserManager<User> userManager)
        {
            var users = await userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                await userManager.DeleteAsync(user);
            }

            await Initialize(userManager);

        }
    }
}


