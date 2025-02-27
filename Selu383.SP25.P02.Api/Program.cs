
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Microsoft.AspNetCore.SpaServices;
using Selu383.SP25.P02.Api.Features.Roles;
using Selu383.SP25.P02.Api.Features.Users;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

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
                    options.Cookie.Name = "AuthenticationCookie";
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.LoginPath = "/api/authentication/login";
                    // ReturnUrlParameter requires 
                    //using Microsoft.AspNetCore.Authentication.Cookies;
                    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                    options.SlidingExpiration = true;
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    };
                }
            });
        
            builder.Services.AddControllers();

            // Register Swagger services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CMPS-383_P02_G02", Version = "v1" });
            });


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
                await db.Database.MigrateAsync(); // Apply migrations

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                await SeedRoles.Initialize(roleManager); 

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                await SeedUsers.Initialize(userManager);

                if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_TEST") == "true")
                {
                    
                    SeedTheaters.Initialize(scope.ServiceProvider);
                }

                
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMPS-383_P02_G02");
                    c.RoutePrefix = "swagger"; 
                });

                app.UseDeveloperExceptionPage();

                app.UseSpa(x => x.UseProxyToSpaDevelopmentServer("http://localhost:5173"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            
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

            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Incoming Request: {context.Request.Method} {context.Request.Path}");
                await next();
            });


            app.Run();
        }
    }
}
