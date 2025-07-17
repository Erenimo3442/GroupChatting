using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests
{
    public class UserServiceTests
    {
        private static AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Use a unique name for each test
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            var dbContext = GetDbContext();
            var userService = new UserService(dbContext);
            var username = "testuser";
            var password = "password123";

            // Act
            await userService.RegisterAsync(username, password);

            // Assert
            var savedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.NotNull(savedUser);
            Assert.Equal(username, savedUser.Username);
        }
    }
}
