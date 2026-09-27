using E_commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260924200000_AddOrderPaymentDetails")]
public partial class AddOrderPaymentDetails : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "PaymentIntentKey", table: "Orders", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PaymentStatus", table: "Orders", type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Unpaid");
        migrationBuilder.AddColumn<string>(name: "PaymentUrl", table: "Orders", type: "nvarchar(1000)", maxLength: 1000, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PaymentIntentKey", table: "Orders");
        migrationBuilder.DropColumn(name: "PaymentStatus", table: "Orders");
        migrationBuilder.DropColumn(name: "PaymentUrl", table: "Orders");
    }
}
