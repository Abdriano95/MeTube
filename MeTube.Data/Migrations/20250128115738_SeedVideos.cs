using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeTube.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedVideos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "Password" },
                values: new object[] { "john.doe@example.com", "pwd123" });

            migrationBuilder.InsertData(
                table: "Videos",
                columns: new[] { "Id", "DateUploaded", "Description", "Genre", "Title", "UserId", "VideoUrl" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "An introduction to C# programming for beginners.", "Education", "Learning C# Basics", 1, "https://example.com/video1" },
                    { 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Learn advanced LINQ techniques to simplify your C# code.", "Tech", "Advanced LINQ Tips", 1, "https://example.com/video2" },
                    { 3, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Explore .NET MAUI and learn how to build cross-platform apps.", "Education", "Introduction to .NET MAUI", 2, "https://example.com/video3" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Videos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Videos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Videos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "Password" },
                values: new object[] { "pwd123", "john.doe@example.com" });
        }
    }
}
