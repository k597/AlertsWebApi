using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertsWebApi.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", maxLength: 50, nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IpAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Blacklisted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SourceType = table.Column<int>(type: "INTEGER", maxLength: 50, nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpAddresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertIpAddresses",
                columns: table => new
                {
                    AlertId = table.Column<int>(type: "INTEGER", nullable: false),
                    IpAddressId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertIpAddresses", x => new { x.AlertId, x.IpAddressId });
                    table.ForeignKey(
                        name: "FK_AlertIpAddresses_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertIpAddresses_IpAddresses_IpAddressId",
                        column: x => x.IpAddressId,
                        principalTable: "IpAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertIpAddresses_IpAddressId",
                table: "AlertIpAddresses",
                column: "IpAddressId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertIpAddresses");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "IpAddresses");
        }
    }
}
