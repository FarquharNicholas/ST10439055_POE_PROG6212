using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10439055_POE_PROG6212.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Lecturers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Lecturers",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordHash",
                table: "Lecturers",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordSalt",
                table: "Lecturers",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Lecturers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Lecturers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"UPDATE ""Lecturers""
SET ""IsActive"" = 1,
    ""Role"" = CASE WHEN ""Role"" = 0 THEN 1 ELSE ""Role"" END,
    ""CreatedAt"" = COALESCE(""CreatedAt"", CURRENT_TIMESTAMP);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Lecturers");
        }
    }
}
