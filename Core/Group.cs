namespace Core
{
    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public ICollection<GroupMember> Memberships { get; set; } = new List<GroupMember>();
    }
}
