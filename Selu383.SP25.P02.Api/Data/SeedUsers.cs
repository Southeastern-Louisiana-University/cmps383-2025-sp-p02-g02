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
            bool isTestEnvironment = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_TEST") == "true";

            var usersExist = await userManager.Users.AnyAsync();
            if (isTestEnvironment || !usersExist)
            {
                await ensureUserExists(userManager, "galkadi", "Password123!", "Admin");
                await ensureUserExists(userManager, "bob", "Password123!", "User");
                await ensureUserExists(userManager, "sue", "Password123!", "User");
            }
        }

        private static async Task ensureUserExists(UserManager<User> userManager, string username, string password, string role)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user != null)
            {
                return; // User already exists
            }

            user = new User { UserName = username };
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}


