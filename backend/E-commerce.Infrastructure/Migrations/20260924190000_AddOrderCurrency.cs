using E_commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260924190000_AddOrderCurrency")]
public partial class AddOrderCurrency : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Currency", table: "Orders", type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "SAR");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Currency", table: "Orders");
    }
}
