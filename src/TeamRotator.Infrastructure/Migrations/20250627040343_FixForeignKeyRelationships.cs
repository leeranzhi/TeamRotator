using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamRotator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeyRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Members");

            migrationBuilder.AlterColumn<string>(
                name: "RotationRule",
                table: "Tasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "TaskName",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemberId1",
                table: "TaskAssignments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaskId1",
                table: "TaskAssignments",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SlackId",
                table: "Members",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Host",
                table: "Members",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_MemberId1",
                table: "TaskAssignments",
                column: "MemberId1");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_TaskId1",
                table: "TaskAssignments",
                column: "TaskId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_Members_MemberId1",
                table: "TaskAssignments",
                column: "MemberId1",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_Tasks_TaskId1",
                table: "TaskAssignments",
                column: "TaskId1",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_Members_MemberId1",
                table: "TaskAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_Tasks_TaskId1",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_MemberId1",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_TaskId1",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "TaskName",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "MemberId1",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "TaskId1",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "Host",
                table: "Members");

            migrationBuilder.AlterColumn<string>(
                name: "RotationRule",
                table: "Tasks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Tasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SlackId",
                table: "Members",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Members",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
