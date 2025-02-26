using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Selu383.SP25.P02.Api.Features.Users;
using Selu383.SP25.P02.Api.Features.Roles;
using Microsoft.EntityFrameworkCore;

namespace Selu383.SP25.P02.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;

        public UsersController(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            
            if (!User.IsInRole("Admin"))
            {
                return Forbid("Only admins can create users."); 
            }

            //at least one role must be provided
            if (createUserDto.Roles == null || createUserDto.Roles.Length == 0)
            {
                return BadRequest("At least one role must be provided.");
            }

            // Only allow unique user names
            var existingUser = await userManager.FindByNameAsync(createUserDto.UserName);
            if (existingUser != null)
            {
                return BadRequest("Username is already taken.");
            }

            //verify valid roles only
            var validRoles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
            var invalidRoles = createUserDto.Roles.Except(validRoles).ToList();
            if (invalidRoles.Any())
            {
                return BadRequest($"Invalid roles: {string.Join(", ", invalidRoles)}");
            }

            //create
            var user = new User
            {
                UserName = createUserDto.UserName
            };

            var result = await userManager.CreateAsync(user, createUserDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Assign roles
            await userManager.AddToRolesAsync(user, createUserDto.Roles);

           
            return Ok(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = createUserDto.Roles
            });
        }
    }
}

