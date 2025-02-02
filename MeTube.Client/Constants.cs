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
        public static string DeleteUser = $"{BaseUrl}/user";
        public static string UpdateUser = $"{BaseUrl}/user";
        public static string ChangeRole = $"{BaseUrl}/user/changeRole/{{0}}";

        // Video endpoints
        public static string VideoBaseUrl = $"{BaseUrl}/Video";
        public static string VideoGetAllUrl = VideoBaseUrl;
        public static string VideoGetByIdUrl = $"{VideoBaseUrl}";
        public static string VideoUploadUrl = VideoBaseUrl;
        public static string VideoStreamUrl = $"{VideoBaseUrl}/stream";
        public static string VideoUpdateUrl = $"{VideoBaseUrl}/{0}";
        public static string VideoUpdateFileUrl = $"{VideoBaseUrl}/{0}/file";
        public static string VideoUpdateThumbnailUrl = $"{VideoBaseUrl}/{0}/thumbnail";
        public static string VideoDeleteUrl = $"{VideoBaseUrl}/{0}";
        public static string VideoResetThumbnailUrl = $"{VideoBaseUrl}/{0}/default-thumbnail";



    }
}
