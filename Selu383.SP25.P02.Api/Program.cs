
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Microsoft.AspNetCore.SpaServices;
using Selu383.SP25.P02.Api.Features.Roles;
using Selu383.SP25.P02.Api.Features.Users;


namespace Selu383.SP25.P02.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container. Register DataContext.
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext") ?? throw new InvalidOperationException("Connection string 'DataContext' not found.")));

            // Add Identity <user, role>
            builder.Services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<DataContext>();

            // Add cookie authentication
            builder.Services.ConfigureApplicationCookie(options => {
                {
                    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                    options.Cookie.Name = "YourAppCookieName";
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.LoginPath = "/Identity/Account/Login";
                    // ReturnUrlParameter requires 
                    //using Microsoft.AspNetCore.Authentication.Cookies;
                    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                    options.SlidingExpiration = true;
                }
            });
        
            builder.Services.AddControllers();

            // Add OpenAPI (Swagger)
            builder.Services.AddOpenApi();

            // Add CORS policy to allow requests from frontend
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader());
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                await db.Database.MigrateAsync();
                SeedTheaters.Initialize(scope.ServiceProvider);

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                await SeedRoles.Initialize(roleManager); // Create roles first
                await SeedUsers.Initialize(userManager); // Then create users and assign roles

            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseDeveloperExceptionPage();
                app.UseSpa(x => x.UseProxyToSpaDevelopmentServer("http://localhost:5173"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseStaticFiles(); // Serve static files (React build)
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            if (!app.Environment.IsDevelopment())
            {
                app.MapFallbackToFile("/index.html"); // Serve React app in production
            }

            app.Run();
        }
    }
}
