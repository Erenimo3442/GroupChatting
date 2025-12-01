using Core;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<TempFileUpload> TempFileUploads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GroupMember>().HasKey(gm => new { gm.UserId, gm.GroupId });

            // Define the relationship from GroupMember to User
            modelBuilder
                .Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.Memberships)
                .HasForeignKey(gm => gm.UserId);

            // Define the relationship from GroupMember to Group
            modelBuilder
                .Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Memberships)
                .HasForeignKey(gm => gm.GroupId);
        }
    }
}
