using System;
using System.Threading.Tasks;
using Application;
using Application.Dtos;
using Application.Exceptions;
using Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests
{
    public class GroupServiceTests
    {
        private async Task<(
            AppDbContext,
            User admin,
            User regularUser,
            Group privateGroup
        )> GetSeededDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new AppDbContext(options);

            // Arrange: Create users and a private group
            var adminUser = new User { Id = Guid.NewGuid(), Username = "admin" };
            var regularUser = new User { Id = Guid.NewGuid(), Username = "user" };
            var invitedUser = new User { Id = Guid.NewGuid(), Username = "invited" };

            var privateGroup = new Group
            {
                Id = Guid.NewGuid(),
                Name = "Private Group",
                IsPublic = false,
            };

            privateGroup.Memberships.Add(
                new GroupMember
                {
                    Group = privateGroup,
                    User = adminUser,
                    Role = "Admin",
                    MembershipStatus = "Active",
                }
            );
            privateGroup.Memberships.Add(
                new GroupMember
                {
                    Group = privateGroup,
                    User = invitedUser,
                    Role = "Member",
                    MembershipStatus = "Invited",
                }
            );

            await dbContext.Users.AddRangeAsync(adminUser, regularUser, invitedUser);
            await dbContext.Groups.AddAsync(privateGroup);

            await dbContext.SaveChangesAsync();
            return (dbContext, adminUser, regularUser, privateGroup);
        }

        [Fact]
        public async Task InviteUserToGroupAsync_WhenInviterIsNotAdmin_ThrowsException()
        {
            // Arrange
            var (dbContext, admin, regularUser, privateGroup) = await GetSeededDbContextAsync();
            var service = new GroupService(dbContext);
            var dto = new InviteUserToGroupDto { InviteeId = regularUser.Id };

            // Act & Assert
            // A regular user (not an admin) attempts to invite someone
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                service.InviteUserToGroupAsync(privateGroup.Id, regularUser.Id, dto)
            );
        }

        [Fact]
        public async Task ApplyToGroupAsync_WhenGroupIsPrivate_CreatesPendingApplication()
        {
            // Arrange
            var (dbContext, admin, regularUser, privateGroup) = await GetSeededDbContextAsync();
            var service = new GroupService(dbContext);

            // Act
            await service.ApplyToGroupAsync(privateGroup.Id, regularUser.Id);

            // Assert
            var application = await dbContext.GroupMembers.FirstOrDefaultAsync(gm =>
                gm.GroupId == privateGroup.Id && gm.UserId == regularUser.Id
            );
            Assert.NotNull(application);
            Assert.Equal("PendingApproval", application.MembershipStatus);
        }

        [Fact]
        public async Task AcceptGroupInvitationAsync_WithValidInvitation_ChangesStatusToActive()
        {
            // Arrange
            var (dbContext, admin, regularUser, privateGroup) = await GetSeededDbContextAsync();
            var service = new GroupService(dbContext);
            var invitedUserId = dbContext.Users.First(u => u.Username == "invited").Id;

            // Act
            await service.AcceptGroupInvitationAsync(privateGroup.Id, invitedUserId);

            // Assert
            var member = await dbContext.GroupMembers.FirstAsync(gm => gm.UserId == invitedUserId);
            Assert.Equal("Active", member.MembershipStatus);
        }

        [Fact]
        public async Task JoinPublicGroupAsync_WhenGroupIsPrivate_ThrowsException()
        {
            // Arrange
            var (dbContext, admin, regularUser, privateGroup) = await GetSeededDbContextAsync();
            var service = new GroupService(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.JoinPublicGroupAsync(privateGroup.Id, regularUser.Id)
            );
        }
    }
}
