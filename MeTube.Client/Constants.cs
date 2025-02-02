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
        // Video GET endpoints
        public static string VideoBaseUrl = $"{BaseUrl}/Video";
        public static string VideoGetByIdUrl = $"{VideoBaseUrl}";
        public static string VideoStreamUrl = $"{VideoBaseUrl}/stream";
        public static string VideoGetUsersVideos = $"{VideoBaseUrl}/user";

        // Video POST endpoints
        public static string VideoUploadUrl = VideoBaseUrl;

        // Video PUT endpoints
        public static string VideoUpdateUrl = $"{VideoBaseUrl}";
        public static string VideoUpdateFileUrl = $"{VideoBaseUrl}";
        public static string VideoUpdateThumbnailUrl = $"{VideoBaseUrl}";
        public static string VideoResetThumbnailUrl = $"{VideoBaseUrl}/{0}/default-thumbnail";

        // Video DELETE endpoints
        public static string VideoDeleteUrl = $"{VideoBaseUrl}/{0}";

        //public static string BaseUrl = $"{Constants.BaseUrl}/Video";

        //// GET endpoints
        //public static string VideoGetAllUrl = BaseUrl;
        //public static string VideoGetByIdUrl = $"{BaseUrl}/{0}";
        //public static string VideoGetStreamUrl = $"{BaseUrl}/stream/{0}";
        //public static string VideoGetByUserIdUrl = $"{BaseUrl}/user/{0}";

    }
}
