using System.Threading.Tasks;
using Application;
using Application.Dtos;
using Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Infrastructure.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IConfiguration> _configMock;

        public UserServiceTests()
        {
            _tokenServiceMock = new Mock<ITokenService>();
            _configMock = new Mock<IConfiguration>();
        }

        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            var dbContext = GetDbContext();
            // We don't need mocks for this test, so we can pass null
            var userService = new UserService(
                dbContext,
                _tokenServiceMock.Object,
                _configMock.Object
            );
            var username = "testuser";
            var password = "password123";

            // Act
            await userService.RegisterAsync(
                new RegisterDto { Username = username, Password = password }
            );

            // Assert
            var savedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.NotNull(savedUser);
            Assert.Equal(username, savedUser.Username);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsTokensAndSavesRefreshToken()
        {
            // Arrange
            var dbContext = GetDbContext();
            var username = "testuser";
            var password = "password123";
            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            };
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            _tokenServiceMock
                .Setup(s => s.CreateToken(It.IsAny<User>()))
                .Returns("fake_access_token");

            var userService = new UserService(
                dbContext,
                _tokenServiceMock.Object,
                _configMock.Object
            );

            // Act
            var result = await userService.LoginAsync(
                new LoginDto { Username = username, Password = password }
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fake_access_token", result.AccessToken);
            Assert.NotEmpty(result.RefreshToken);

            var userInDb = await dbContext.Users.FirstAsync();
            Assert.Equal(result.RefreshToken, userInDb.RefreshToken);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            var dbContext = GetDbContext();
            var secretKey = "a_super_secret_key_that_is_long_enough_for_sha256";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                RefreshToken = "valid_refresh_token",
                TokenExpires = DateTime.UtcNow.AddDays(1),
            };
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            // 1. Setup the IConfiguration mock
            _configMock.Setup(c => c["AppSettings:Token"]).Returns(secretKey);

            // 2. Setup the ITokenService mock to return a new token
            _tokenServiceMock
                .Setup(s => s.CreateToken(It.IsAny<User>()))
                .Returns("new_fake_access_token");

            // 3. Create a valid (but notionally expired) access token to pass to the method
            var tokenServiceForTest = new TokenService(_configMock.Object);
            var oldAccessToken = tokenServiceForTest.CreateToken(user);

            var userService = new UserService(
                dbContext,
                _tokenServiceMock.Object,
                _configMock.Object
            );
            var refreshTokenDto = new RefreshTokenDto
            {
                AccessToken = oldAccessToken,
                RefreshToken = "valid_refresh_token",
            };

            // Act
            var result = await userService.RefreshTokenAsync(refreshTokenDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new_fake_access_token", result.AccessToken);
            Assert.NotEqual("valid_refresh_token", result.RefreshToken); // Ensure the refresh token was rotated

            var userInDb = await dbContext.Users.FirstAsync();
            Assert.Equal(result.RefreshToken, userInDb.RefreshToken); // Ensure the new token was saved
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ThrowsException()
        {
            // Arrange
            var dbContext = GetDbContext();
            var existingUser = new User { Username = "testuser" };
            await dbContext.Users.AddAsync(existingUser);
            await dbContext.SaveChangesAsync();

            var userService = new UserService(
                dbContext,
                _tokenServiceMock.Object,
                _configMock.Object
            );
            var registerDto = new RegisterDto { Username = "testuser", Password = "password123" };

            // Act & Assert
            // Verify that the correct exception is thrown when trying to register with a duplicate username
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                userService.RegisterAsync(registerDto)
            );
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            var dbContext = GetDbContext();
            var username = "testuser";
            var password = "password123";
            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            };
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            var userService = new UserService(
                dbContext,
                _tokenServiceMock.Object,
                _configMock.Object
            );
            var loginDto = new LoginDto { Username = username, Password = "wrong_password" };

            // Act
            var result = await userService.LoginAsync(loginDto);

            // Assert
            Assert.Null(result);
        }
    }
}
