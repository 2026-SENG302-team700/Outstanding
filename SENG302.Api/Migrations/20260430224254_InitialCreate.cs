using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SENG302.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    FileKey = table.Column<string>(type: "text", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskItems",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskListId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false),
                    creationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskItems", x => x.TaskId);
                });

            migrationBuilder.CreateTable(
                name: "TaskLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    NextId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PasswordKey = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    ProfilePicture = table.Column<int>(type: "integer", nullable: false),
                    ProfilePictureOffsetX = table.Column<float>(type: "real", nullable: false),
                    ProfilePictureOffsetY = table.Column<float>(type: "real", nullable: false),
                    ProfilePictureZoom = table.Column<float>(type: "real", nullable: false),
                    OneTimeCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    CodeGenerationTime = table.Column<long>(type: "bigint", nullable: false),
                    TimeCreated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    ProfanityFiltering = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomFiles");

            migrationBuilder.DropTable(
                name: "TaskItems");

            migrationBuilder.DropTable(
                name: "TaskLists");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
