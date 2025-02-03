using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeTube.Data.Migrations
{
    /// <inheritdoc />
    public partial class initialcreate : Migration
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "john.doe@example.com", "pwd123", "Admin", "johndoe1" },
                    { 2, "jane.doe@example.com", "pwd456", "User", "janedoe2" }
                });

            migrationBuilder.InsertData(
                table: "Videos",
                columns: new[] { "Id", "BlobName", "DateUploaded", "Description", "Genre", "ThumbnailUrl", "Title", "UserId", "VideoUrl" },
                values: new object[,]
                {
                    { 1, "youtube_OUUlO8fQOfE_1920x1080_h264.mp4", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Should I learn a JavaScript framework or concentrate on mastering Blazor? What is the future of Blazor? Is Microsoft invested in making Blazor great? We will answer these questions in today's Dev Questions episode.   Website: https://www.iamtimcorey.com/", "Programming", "https://looplegionmetube20250129.blob.core.windows.net/thumbnails/whatisthefutureofblazor.jpg", "164. What is the Future of Blazor? Should I Learn Blazor?", 1, "https://looplegionmetube20250129.blob.core.windows.net/videos/youtube_OUUlO8fQOfE_1920x1080_h264.mp4" },
                    { 2, "videoplayback (1).mp4", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "string", "string", "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e8/YouTube_Diamond_Play_Button.png/1200px-YouTube_Diamond_Play_Button.png", "string", 2, "https://looplegionmetube20250129.blob.core.windows.net/videos/videoplayback%20%281%29.mp4" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Videos_UserId",
                table: "Videos",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
