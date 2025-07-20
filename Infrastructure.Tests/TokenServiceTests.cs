using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Core;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Infrastructure.Tests
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _configMock = new Mock<IConfiguration>();

            // Arrange: Set up the mock configuration for the token secret and lifetime
            _configMock
                .Setup(c => c["AppSettings:Token"])
                .Returns("a_test_secret_key_that_is_long_enough_for_sha256");
            _configMock.Setup(c => c["AppSettings:AccessTokenLifetimeDays"]).Returns("7");

            _tokenService = new TokenService(_configMock.Object);
        }

        [Fact]
        public void CreateToken_WithValidUser_ReturnsValidJwtTokenWithCorrectClaims()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Username = "testuser" };

            // Act
            var tokenString = _tokenService.CreateToken(user);

            // Assert
            Assert.False(string.IsNullOrEmpty(tokenString));

            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(tokenString);

            var userIdClaim = decodedToken
                .Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)
                ?.Value;
            var usernameClaim = decodedToken
                .Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)
                ?.Value;

            Assert.Equal(user.Id.ToString(), userIdClaim);
            Assert.Equal(user.Username, usernameClaim);
        }
    }
}
