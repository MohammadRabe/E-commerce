using E_commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260924170000_RequireOrderPhoneNumber")]
public partial class RequireOrderPhoneNumber : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE [Orders]
            SET [ShippingPhoneNumber] = COALESCE(
                NULLIF(LTRIM(RTRIM([Orders].[ShippingPhoneNumber])), ''),
                NULLIF(LTRIM(RTRIM([AspNetUsers].[Phone])), ''),
                NULLIF(LTRIM(RTRIM([AspNetUsers].[PhoneNumber])), ''),
                N'غير محدد')
            FROM [Orders]
            LEFT JOIN [AspNetUsers] ON [Orders].[UserId] = [AspNetUsers].[Id]
            WHERE [Orders].[ShippingPhoneNumber] IS NULL OR LTRIM(RTRIM([Orders].[ShippingPhoneNumber])) = '';
            """);

        migrationBuilder.AlterColumn<string>(
            name: "ShippingPhoneNumber",
            table: "Orders",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30,
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<string>(
            name: "ShippingPhoneNumber",
            table: "Orders",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30);
}
