using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZebraIoTConnector.Persistence.Migrations
{
    public partial class AddAssetMovementDirection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Direction",
                table: "AssetMovements",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direction",
                table: "AssetMovements");
        }
    }
}
