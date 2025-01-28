namespace MeTube.Client
{
    public static class Constants
    {
        public static string LocalhostUrl = "localhost";

        public static string Scheme = "https"; // or http
        public static string Port = "5001"; // or 5000
        public static string BaseUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api";

        // Specific REST URLs
        public static string UserRegisterUrl = $"{BaseUrl}/user/signup";
        public static string GetAllUsers = $"{BaseUrl}/user/manageUsers";
        public static string GetUserIdByEmail = $"{BaseUrl}/user/userIdFromEmail";
        public static string UserLoginUrl = $"{BaseUrl}/user/login";
        public static string GetUserUrl = $"{BaseUrl}/user/{{0}}";


    }
}
