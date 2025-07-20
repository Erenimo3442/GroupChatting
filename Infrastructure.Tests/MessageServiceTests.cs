using System;
using System.Threading.Tasks;
using Application;
using Application.Dtos;
using Application.Exceptions;
using Core;
using Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Infrastructure.Tests
{
    public class MessageServiceTests
    {
        private readonly Mock<IMongoMessageDbService> _mongoMock;
        private readonly Mock<IHubContext<ChatHub>> _hubMock;

        public MessageServiceTests()
        {
            _mongoMock = new Mock<IMongoMessageDbService>();
            _hubMock = new Mock<IHubContext<ChatHub>>();

            // Setup mock for SignalR hub to avoid errors
            var mockClients = new Mock<IClientProxy>();
            var mockGroups = new Mock<IGroupManager>();
            _hubMock.Setup(h => h.Clients.Group(It.IsAny<string>())).Returns(mockClients.Object);
            _hubMock.Setup(h => h.Groups).Returns(mockGroups.Object);
        }

        private async Task<(
            AppDbContext,
            User admin,
            User activeMember,
            User nonMember,
            Group group
        )> GetSeededDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new AppDbContext(options);

            var adminUser = new User { Id = Guid.NewGuid(), Username = "admin" };
            var activeUser = new User { Id = Guid.NewGuid(), Username = "activeuser" };
            var nonMemberUser = new User { Id = Guid.NewGuid(), Username = "nonmember" }; // Add this user
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };

            group.Memberships.Add(
                new GroupMember
                {
                    Group = group,
                    User = adminUser,
                    Role = "Admin",
                    MembershipStatus = "Active",
                }
            );
            group.Memberships.Add(
                new GroupMember
                {
                    Group = group,
                    User = activeUser,
                    Role = "Member",
                    MembershipStatus = "Active",
                }
            );

            // Add all three users to the database
            await dbContext.Users.AddRangeAsync(adminUser, activeUser, nonMemberUser);
            await dbContext.Groups.AddAsync(group);
            await dbContext.SaveChangesAsync();

            // Update the return tuple
            return (dbContext, adminUser, activeUser, nonMemberUser, group);
        }

        [Fact]
        public async Task SendMessageAsync_WhenUserIsNotActiveMember_ThrowsException()
        {
            // Arrange
            // Get the nonMember from the setup
            var (dbContext, admin, activeMember, nonMember, group) =
                await GetSeededDbContextAsync();
            var service = new MessageService(_mongoMock.Object, _hubMock.Object, dbContext);
            var dto = new SendMessageDto { GroupId = group.Id, Content = "Hello" };

            // Act & Assert
            // Use the nonMember's Id to test the security check
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.SendMessageAsync(dto, nonMember.Id)
            );
        }

        [Fact]
        public async Task SendMessageAsync_WhenUserIsActiveMember_CreatesMessageInMongoDb()
        {
            // Arrange
            var (dbContext, admin, activeMember, nonMember, group) =
                await GetSeededDbContextAsync();
            var service = new MessageService(_mongoMock.Object, _hubMock.Object, dbContext);
            var dto = new SendMessageDto { GroupId = group.Id, Content = "Hello World" };

            // Act
            await service.SendMessageAsync(dto, activeMember.Id);

            // Assert
            // Verify that the CreateAsync method on our fake MongoDB service was called exactly once.
            _mongoMock.Verify(m => m.CreateAsync(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public async Task SendMessageAsync_WhenMessageHasFile_CreatesMessageWithFileUrl()
        {
            // Arrange
            var (dbContext, admin, activeMember, nonMember, group) =
                await GetSeededDbContextAsync();
            var service = new MessageService(_mongoMock.Object, _hubMock.Object, dbContext);
            var dto = new SendMessageDto
            {
                GroupId = group.Id,
                Content = "[File]",
                FileUrl = "/uploads/test.jpg",
                MimeType = "image/jpeg",
            };

            Message? capturedMessage = null;

            // Setup the mock to capture the message that is passed to it
            _mongoMock
                .Setup(m => m.CreateAsync(It.IsAny<Message>()))
                .Callback<Message>(m => capturedMessage = m);

            // Act
            await service.SendMessageAsync(dto, activeMember.Id);

            // Assert
            Assert.NotNull(capturedMessage);
            Assert.Equal("/uploads/test.jpg", capturedMessage.FileUrl);
            Assert.Equal("image/jpeg", capturedMessage.MimeType);
        }
    }
}
