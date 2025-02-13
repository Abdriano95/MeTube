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
        public static string GetAllUsersDetails = $"{BaseUrl}/user/manageUsersDetails";
        public static string GetUserIdByEmail = $"{BaseUrl}/user/userIdFromEmail";
        public static string UserLoginUrl = $"{BaseUrl}/user/login";
        public static string GetUserUrl = $"{BaseUrl}/user/{{0}}";
        public static string DeleteUser = $"{BaseUrl}/user";
        public static string UpdateUser = $"{BaseUrl}/user";
        public static string ChangeRole = $"{BaseUrl}/user/changeRole/{{0}}";
        public static string CheckUserExistsUrl = $"{BaseUrl}/User/exists";
        public static string GetLogedInUsername = $"{BaseUrl}/user/logedInUsername";

        // Video endpoints
        public static string VideoBaseUrl = $"{BaseUrl}/Video";

        // GET
        public static string VideoGetAllUrl = VideoBaseUrl; // GET api/Video
        public static string VideoGetByIdUrl(int id) => $"{VideoBaseUrl}/{id}"; // GET api/Video/{id}

        public static string VideoGetRecommendedUrl = $"{VideoBaseUrl}/recommended";
        public static string VideoStreamUrl(int id) => $"{VideoBaseUrl}/stream/{id}"; // GET api/Video/stream/{id}
        public static string VideoGetByUserUrl = $"{VideoBaseUrl}/user"; // GET api/Video/user
        public static string VideoGetUploaderUsernameUrl(int videoId) => $"{VideoBaseUrl}/username/{videoId}";

        // POST
        public static string VideoUploadUrl = VideoBaseUrl; // POST api/Video

        // PUT
        public static string VideoUpdateUrl(int id) => $"{VideoBaseUrl}/{id}"; // PUT api/Video/{id}
        public static string VideoUpdateFileUrl(int id) => $"{VideoBaseUrl}/{id}/file"; // PUT api/Video/{id}/file
        public static string VideoUpdateThumbnailUrl(int id) => $"{VideoBaseUrl}/{id}/thumbnail"; // PUT api/Video/{id}/thumbnail
        public static string VideoResetThumbnailUrl(int id) => $"{VideoBaseUrl}/{id}/default-thumbnail"; // PUT api/Video/{id}/default-thumbnail

        // DELETE
        public static string VideoDeleteUrl(int id) => $"{VideoBaseUrl}/{id}"; // DELETE api/Video/{id}

        public static string LikeRemoveAdminUrl(int videoId, int userId) => $"{LikeBaseUrl}/{videoId}/{userId}";


        // Like endpoints
        public static string LikeBaseUrl = $"{BaseUrl}/Like";

        // GET
        public static string LikeGetAllUrl = LikeBaseUrl;
        public static string LikeGetByVideoIdUrl(int videoId) => $"{LikeBaseUrl}/{videoId}";
        public static string LikeGetForVideoUrl(int videoId) => $"{LikeBaseUrl}/video/{videoId}";

        // POST
        public static string LikeAddUrl = LikeBaseUrl;

        // DELETE
        public static string LikeRemoveUrl = LikeBaseUrl;

        // History endpoints
        public static string HistoryBaseUrl = $"{BaseUrl}/History";

        // GET
        public static string HistoryGetAllUrl = HistoryBaseUrl;

        // POST
        public static string HistoryAddUrl = HistoryBaseUrl;

        // History endpoints for Admin
        public static string HistoryAdminBaseUrl = $"{BaseUrl}/History/admin";
        // GET
        public static string HistoryAdminGetByUserIdUrl(int userId) => $"{HistoryAdminBaseUrl}/user/{userId}";

        // POST
        public static string HistoryAdminAddUrl = HistoryAdminBaseUrl;

        // PUT
        public static string HistoryAdminUpdateUrl(int historyId) => $"{HistoryAdminBaseUrl}/{historyId}";

        // DELETE
        public static string HistoryAdminDeleteUrl(int historyId) => $"{HistoryAdminBaseUrl}/{historyId}";

        // Comment endpoints
        public static string CommentBaseUrl = $"{BaseUrl}/comments";

        // GET
        public static string CommentGetByVideoIdUrl(int videoId) => $"{CommentBaseUrl}/video/{videoId}";
        public static string CommentGetPosterUsernameUrl(int userId) => $"{CommentBaseUrl}/username/{userId}";

        // POST
        public static string CommentAddUrl = CommentBaseUrl;

        // PUT
        public static string CommentUpdateUrl(int commentId) => $"{CommentBaseUrl}/{commentId}";

        // DELETE
        public static string CommentDeleteUrl(int commentId) => $"{CommentBaseUrl}/{commentId}";
    }
}
