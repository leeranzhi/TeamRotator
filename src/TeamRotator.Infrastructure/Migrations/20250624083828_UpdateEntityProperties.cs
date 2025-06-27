using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamRotator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntityProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaskName",
                table: "Tasks",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Host",
                table: "Members",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tasks",
                newName: "TaskName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Members",
                newName: "Host");
        }
    }
}
