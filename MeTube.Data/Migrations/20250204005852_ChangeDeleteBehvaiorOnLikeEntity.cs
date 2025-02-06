using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeTube.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDeleteBehvaiorOnLikeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Users_UserID",
                table: "Likes");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Users_UserID",
                table: "Likes",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Users_UserID",
                table: "Likes");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Users_UserID",
                table: "Likes",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
