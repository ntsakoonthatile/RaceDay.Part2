using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Tests
{
    public class CustomWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                    typeof(DbContextOptions<RaceDayDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<RaceDayDbContext>(options =>
                {
                    options.UseInMemoryDatabase("RaceDayTestDatabase");
                });

                var serviceProvider =
                    services.BuildServiceProvider();

                using var scope =
                    serviceProvider.CreateScope();

                var db =
                    scope.ServiceProvider
                        .GetRequiredService<RaceDayDbContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.EventTypes.AddRange(
                    new EventType
                    {
                        EventTypeId = 1,
                        TypeName = "Run"
                    },
                    new EventType
                    {
                        EventTypeId = 2,
                        TypeName = "Walk"
                    },
                    new EventType
                    {
                        EventTypeId = 3,
                        TypeName = "Cycle"
                    });

                db.Users.AddRange(
                    new User
                    {
                        UserId = 1,
                        FirstName = "Test",
                        LastName = "Organiser",
                        Email = "organiser@test.com",
                        PasswordHash =
                            BCrypt.Net.BCrypt.HashPassword(
                                "Password123!"),
                        Role = "Organiser"
                    },
                    new User
                    {
                        UserId = 2,
                        FirstName = "Test",
                        LastName = "Participant",
                        Email = "participant@test.com",
                        PasswordHash =
                            BCrypt.Net.BCrypt.HashPassword(
                                "Password123!"),
                        Role = "Participant"
                    });

                db.SaveChanges();
            });
        }
    }
}
