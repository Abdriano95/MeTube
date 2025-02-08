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
        public static string CheckUserExistsUrl = $"{BaseUrl}/User/exists";

        // Video endpoints
        public static string VideoBaseUrl = $"{BaseUrl}/Video";

        // GET
        public static string VideoGetAllUrl = VideoBaseUrl; // GET api/Video
        public static string VideoGetByIdUrl(int id) => $"{VideoBaseUrl}/{id}"; // GET api/Video/{id}
        public static string VideoStreamUrl(int id) => $"{VideoBaseUrl}/stream/{id}"; // GET api/Video/stream/{id}
        public static string VideoGetByUserUrl = $"{VideoBaseUrl}/user"; // GET api/Video/user

        // POST
        public static string VideoUploadUrl = VideoBaseUrl; // POST api/Video

        // PUT
        public static string VideoUpdateUrl(int id) => $"{VideoBaseUrl}/{id}"; // PUT api/Video/{id}
        public static string VideoUpdateFileUrl(int id) => $"{VideoBaseUrl}/{id}/file"; // PUT api/Video/{id}/file
        public static string VideoUpdateThumbnailUrl(int id) => $"{VideoBaseUrl}/{id}/thumbnail"; // PUT api/Video/{id}/thumbnail
        public static string VideoResetThumbnailUrl(int id) => $"{VideoBaseUrl}/{id}/default-thumbnail"; // PUT api/Video/{id}/default-thumbnail

        // DELETE
        public static string VideoDeleteUrl(int id) => $"{VideoBaseUrl}/{id}"; // DELETE api/Video/{id}


    }
}
