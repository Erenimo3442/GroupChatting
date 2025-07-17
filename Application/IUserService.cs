namespace Application
{
    public interface IUserService
    {
        Task<User> RegisterAsync(string username, string password);
        Task<User?> LoginAsync(string username, string password);
    }
}
