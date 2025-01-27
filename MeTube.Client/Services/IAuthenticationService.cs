namespace MeTube.Client.Services
{
    public interface IAuthenticationService
    {
        Task<bool> IsUserAuthenticated();
        Task Login(string token);
        Task Logout();
    }
}
