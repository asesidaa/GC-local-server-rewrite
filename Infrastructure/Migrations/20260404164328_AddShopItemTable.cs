using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shop_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    item_type = table.Column<int>(type: "INTEGER", nullable: false),
                    item_id = table.Column<int>(type: "INTEGER", nullable: false),
                    coin_cost = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_item", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shop_item_item_type_item_id",
                table: "shop_item",
                columns: new[] { "item_type", "item_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shop_item");
        }
    }
}
