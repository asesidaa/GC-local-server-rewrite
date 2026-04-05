using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnlockAndCoinTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "default_unlock_state",
                columns: table => new
                {
                    item_type = table.Column<int>(type: "INTEGER", nullable: false),
                    unlocked_bitset = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_default_unlock_state", x => x.item_type);
                });

            migrationBuilder.CreateTable(
                name: "player_coin",
                columns: table => new
                {
                    card_id = table.Column<long>(type: "INTEGER", nullable: false),
                    current_coins = table.Column<int>(type: "INTEGER", nullable: false),
                    total_coins = table.Column<int>(type: "INTEGER", nullable: false),
                    monthly_coins = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_coin", x => x.card_id);
                });

            migrationBuilder.CreateTable(
                name: "player_unlock_state",
                columns: table => new
                {
                    card_id = table.Column<long>(type: "INTEGER", nullable: false),
                    item_type = table.Column<int>(type: "INTEGER", nullable: false),
                    unlocked_bitset = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_unlock_state", x => new { x.card_id, x.item_type });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "default_unlock_state");

            migrationBuilder.DropTable(
                name: "player_coin");

            migrationBuilder.DropTable(
                name: "player_unlock_state");
        }
    }
}
