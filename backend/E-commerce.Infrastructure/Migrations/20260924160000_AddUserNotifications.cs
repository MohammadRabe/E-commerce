using E_commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260924160000_AddUserNotifications")]
public partial class AddUserNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserNotifications",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RecipientUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                OrderId = table.Column<int>(type: "int", nullable: true),
                Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Link = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                IsRead = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserNotifications", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserNotifications_AspNetUsers_RecipientUserId",
                    column: x => x.RecipientUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserNotifications_RecipientUserId_CreatedAt",
            table: "UserNotifications",
            columns: new[] { "RecipientUserId", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "UserNotifications");
}
