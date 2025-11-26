using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeFit.Migrations
{
    /// <inheritdoc />
    public partial class UserIdRemoval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseEntries_AspNetUsers_UserId",
                table: "ExerciseEntries");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseEntries_UserId",
                table: "ExerciseEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExerciseEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ExerciseEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseEntries_UserId",
                table: "ExerciseEntries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseEntries_AspNetUsers_UserId",
                table: "ExerciseEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
