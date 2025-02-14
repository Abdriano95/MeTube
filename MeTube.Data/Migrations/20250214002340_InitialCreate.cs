using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeTube.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "User")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(2083)", maxLength: 2083, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(2083)", maxLength: 2083, nullable: true),
                    BlobName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateUploaded = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Videos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VideoId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VideoId = table.Column<int>(type: "int", nullable: false),
                    DateWatched = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Histories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Histories_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    VideoID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => new { x.VideoID, x.UserID });
                    table.ForeignKey(
                        name: "FK_Likes_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Likes_Videos_VideoID",
                        column: x => x.VideoID,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "admin@example.com", "adminpwd123", "Admin", "admin" },
                    { 2, "john@example.com", "password123", "User", "john_doe" },
                    { 3, "jane@example.com", "password456", "User", "jane_smith" },
                    { 4, "tech@example.com", "techpass789", "User", "tech_guru" },
                    { 5, "sports@example.com", "sportspass789", "User", "sports_fan" }
                });

            migrationBuilder.InsertData(
                table: "Videos",
                columns: new[] { "Id", "BlobName", "DateUploaded", "Description", "Genre", "ThumbnailUrl", "Title", "UserId", "VideoUrl" },
                values: new object[,]
                {
                    { 1, "541bc83a-c39b-41e0-93e2-b353a5957e0b_faststart.mp4", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Educational video about animation techniques", "Education", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png", "Learn Animation Basics", 1, "https://looplegionmetube20250129.blob.core.windows.net/videos/541bc83a-c39b-41e0-93e2-b353a5957e0b_faststart.mp4" },
                    { 2, "e1543acd-86de-46fb-994f-ee01dc9e4947_faststart.mp4", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fun gaming moments with Big Buck Bunny", "Gaming", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png", "Gaming Adventures", 2, "https://looplegionmetube20250129.blob.core.windows.net/videos/e1543acd-86de-46fb-994f-ee01dc9e4947_faststart.mp4" },
                    { 3, "da06b2df-5389-4f6b-9969-54b538c8062d_faststart.mp4", new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Weekly entertainment highlights", "Entertainment", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png", "Entertainment Weekly", 3, "https://looplegionmetube20250129.blob.core.windows.net/videos/da06b2df-5389-4f6b-9969-54b538c8062d_faststart.mp4" },
                    { 4, "c1e82a0c-a48c-46aa-b33b-bf766ff77449_faststart.mp4", new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Latest in technology", "Technology", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png", "Tech News Today", 4, "https://looplegionmetube20250129.blob.core.windows.net/videos/c1e82a0c-a48c-46aa-b33b-bf766ff77449_faststart.mp4" },
                    { 5, "a6e61589-a50a-482e-b3ec-5ccfb0ec04d6_faststart.mp4", new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Best sports moments of the week", "Sports", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png", "Sports Highlights", 5, "https://looplegionmetube20250129.blob.core.windows.net/videos/a6e61589-a50a-482e-b3ec-5ccfb0ec04d6_faststart.mp4" },
                    { 6, "fc30b0ae-c983-4af5-b9b6-6ec3a1b5609a_faststart.mp4", new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amazing music compilation", "Music", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png", "Music Session", 1, "https://looplegionmetube20250129.blob.core.windows.net/videos/fc30b0ae-c983-4af5-b9b6-6ec3a1b5609a_faststart.mp4" },
                    { 7, "b13bb460-5ec7-4e44-8f06-88c161aa10c6_faststart.mp4", new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Latest news updates", "News", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png", "Breaking News", 2, "https://looplegionmetube20250129.blob.core.windows.net/videos/b13bb460-5ec7-4e44-8f06-88c161aa10c6_faststart.mp4" },
                    { 8, "174c4367-4740-4166-a714-4774d113834d_faststart.mp4", new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Random entertaining content", "Other", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png", "Miscellaneous Fun", 3, "https://looplegionmetube20250129.blob.core.windows.net/videos/174c4367-4740-4166-a714-4774d113834d_faststart.mp4" },
                    { 9, "6495e892-4155-47e1-94b8-cbe317691e4f_faststart.mp4", new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Learning about technology", "Education", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/Big_Buck_Bunny_thumbnail_vlc.png", "Educational Tech", 4, "https://looplegionmetube20250129.blob.core.windows.net/videos/6495e892-4155-47e1-94b8-cbe317691e4f_faststart.mp4" }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "DateAdded", "UserId", "VideoId" },
                values: new object[,]
                {
                    { 1, "Great educational content!", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 1 },
                    { 2, "Very informative video", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 1 },
                    { 3, "Amazing gaming content", new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 2 },
                    { 4, "Love this game!", new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 2 }
                });

            migrationBuilder.InsertData(
                table: "Histories",
                columns: new[] { "Id", "DateWatched", "UserId", "VideoId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 1 },
                    { 2, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 1 },
                    { 3, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 2 },
                    { 4, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 2 }
                });

            migrationBuilder.InsertData(
                table: "Likes",
                columns: new[] { "UserID", "VideoID" },
                values: new object[,]
                {
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 1, 2 },
                    { 3, 2 },
                    { 1, 3 },
                    { 2, 3 },
                    { 5, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VideoId",
                table: "Comments",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_UserId",
                table: "Histories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_VideoId",
                table: "Histories",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserID",
                table: "Likes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_UserId",
                table: "Videos",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
