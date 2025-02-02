using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeTube.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDatabaseWithVideos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Videos",
                columns: new[] { "Id", "BlobName", "DateUploaded", "Description", "Genre", "ThumbnailUrl", "Title", "UserId", "VideoUrl" },
                values: new object[,]
                {
                    { 7, "youtube_OUUlO8fQOfE_1920x1080_h264.mp4", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Should I learn a JavaScript framework or concentrate on mastering Blazor? What is the future of Blazor? Is Microsoft invested in making Blazor great? We will answer these questions in today's Dev Questions episode.   Website: https://www.iamtimcorey.com/", "Programming", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/whatisthefutureofblazor.jpg", "164. What is the Future of Blazor? Should I Learn Blazor?", 1, "https://looplegionmetube20250129.blob.core.windows.net/videos/youtube_OUUlO8fQOfE_1920x1080_h264.mp4" },
                    { 8, "videoplayback (1).mp4", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "string", "string", "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e8/YouTube_Diamond_Play_Button.png/1200px-YouTube_Diamond_Play_Button.png", "string", 2, "https://looplegionmetube20250129.blob.core.windows.net/videos/videoplayback%20%281%29.mp4" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Videos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Videos",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
