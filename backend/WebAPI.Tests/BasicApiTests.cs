using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace WebAPI.Tests
{
    // 1. Create a custom factory that inherits from WebApplicationFactory
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(
                (context, conf) =>
                {
                    // Add in-memory configuration for settings like the JWT key
                    conf.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            {
                                "AppSettings:Token",
                                "a_test_secret_key_that_is_long_enough_for_sha256"
                            },
                        }
                    );
                }
            );

            builder.ConfigureServices(services =>
            {
                // Find and remove the real DbContext registration
                var dbContextDescriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                );

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Add a new DbContext registration that uses an in-memory database
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForApiTesting");
                });

                // Find and remove the real Mongo service
                var mongoServiceDescriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(IMongoMessageDbService)
                );

                if (mongoServiceDescriptor != null)
                {
                    services.Remove(mongoServiceDescriptor);
                }

                // Add a mock Mongo service
                var mongoMock = new Mock<IMongoMessageDbService>();
                services.AddSingleton<IMongoMessageDbService>(mongoMock.Object);
            });
        }
    }

    // 2. Update your test class to use the custom factory
    public class BasicApiTests(CustomWebApplicationFactory factory)
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory = factory;

        [Fact]
        public async Task GetPublicGroups_ReturnsSuccessStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/groups/public");

            // Assert
            response.EnsureSuccessStatusCode(); // Asserts status code is 200-299
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
