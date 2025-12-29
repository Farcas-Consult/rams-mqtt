using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZebraIoTConnector.Persistence.Migrations
{
    public partial class AddAssetTrackingEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GateId",
                table: "Equipments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMobile",
                table: "Equipments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "Equipments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHeartbeat",
                table: "Equipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechnicalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Plant = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StorageLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostCenter = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssetGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObjectType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcquisitionValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TagIdentifier = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastDiscoveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastDiscoveredBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentLocationId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_StorageUnits_CurrentLocationId",
                        column: x => x.CurrentLocationId,
                        principalTable: "StorageUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Gates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gates_StorageUnits_LocationId",
                        column: x => x.LocationId,
                        principalTable: "StorageUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    FromLocationId = table.Column<int>(type: "int", nullable: true),
                    ToLocationId = table.Column<int>(type: "int", nullable: false),
                    GateId = table.Column<int>(type: "int", nullable: true),
                    ReaderId = table.Column<int>(type: "int", nullable: true),
                    ReaderIdString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReadTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetMovements_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetMovements_Equipments_ReaderId",
                        column: x => x.ReaderId,
                        principalTable: "Equipments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetMovements_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetMovements_StorageUnits_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "StorageUnits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetMovements_StorageUnits_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "StorageUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_GateId",
                table: "Equipments",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_AssetId",
                table: "AssetMovements",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_FromLocationId",
                table: "AssetMovements",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_GateId",
                table: "AssetMovements",
                column: "GateId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_ReaderId",
                table: "AssetMovements",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_ReadTimestamp",
                table: "AssetMovements",
                column: "ReadTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_ToLocationId",
                table: "AssetMovements",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetNumber",
                table: "Assets",
                column: "AssetNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CostCenter",
                table: "Assets",
                column: "CostCenter");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CurrentLocationId",
                table: "Assets",
                column: "CurrentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_LastDiscoveredAt",
                table: "Assets",
                column: "LastDiscoveredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Plant",
                table: "Assets",
                column: "Plant");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_TagIdentifier",
                table: "Assets",
                column: "TagIdentifier",
                unique: true,
                filter: "[TagIdentifier] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Gates_LocationId",
                table: "Gates",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Gates_Name",
                table: "Gates",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Gates_GateId",
                table: "Equipments",
                column: "GateId",
                principalTable: "Gates",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Gates_GateId",
                table: "Equipments");

            migrationBuilder.DropTable(
                name: "AssetMovements");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Gates");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_GateId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "GateId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "IsMobile",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "LastHeartbeat",
                table: "Equipments");
        }
    }
}
