using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Selu383.SP25.P02.Api.Features.Theaters;
using Selu383.SP25.P02.Api.Features.Users;

namespace Selu383.SP25.P02.Api.Data
{
    public static class SeedTheaters
    {
        public static void Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            if (context.Theaters.Any())
            {
                return;
            }
            var admin = context.Users.FirstOrDefault(u => u.UserName == "galkadi");
            var t1 = new Theater
            {
                Name = "Theater One",
                Address = "123 Main Street",
                SeatCount = 100,
                Manager = admin
            };
            var t2 = new Theater
            {
                Name = "Theater Two",
                Address = "456 Oak Avenue",
                SeatCount = 200,
                Manager = admin
            };
            var t3 = new Theater
            {
                Name = "Theater Three",
                Address = "789 Elm Road",
                SeatCount = 300,
                Manager = admin
            };
            context.Theaters.AddRange(t1, t2, t3);
            context.SaveChanges();
        }
    }
}