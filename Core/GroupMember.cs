namespace Core
{
    public class GroupMember
    {
        public Guid UserId { get; set; }
        public required User User { get; set; }
        public Guid GroupId { get; set; }
        public required Group Group { get; set; }
        public string Role { get; set; } = "Member";
        public string MembershipStatus { get; set; } = "Active";
    }
}
