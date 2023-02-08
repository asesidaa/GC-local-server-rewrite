using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "card_bdata",
                columns: table => new
                {
                    cardid = table.Column<long>(name: "card_id", type: "INTEGER", nullable: false),
                    bdata = table.Column<string>(type: "TEXT", nullable: true),
                    bdatasize = table.Column<long>(name: "bdata_size", type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_bdata", x => x.cardid);
                });

            migrationBuilder.CreateTable(
                name: "card_detail",
                columns: table => new
                {
                    cardid = table.Column<long>(name: "card_id", type: "INTEGER", nullable: false),
                    pcol1 = table.Column<long>(type: "INTEGER", nullable: false),
                    pcol2 = table.Column<long>(type: "INTEGER", nullable: false),
                    pcol3 = table.Column<long>(type: "INTEGER", nullable: false),
                    scorei1 = table.Column<long>(name: "score_i1", type: "INTEGER", nullable: false),
                    scoreui1 = table.Column<long>(name: "score_ui1", type: "INTEGER", nullable: false),
                    scoreui2 = table.Column<long>(name: "score_ui2", type: "INTEGER", nullable: false),
                    scoreui3 = table.Column<long>(name: "score_ui3", type: "INTEGER", nullable: false),
                    scoreui4 = table.Column<long>(name: "score_ui4", type: "INTEGER", nullable: false),
                    scoreui5 = table.Column<long>(name: "score_ui5", type: "INTEGER", nullable: false),
                    scoreui6 = table.Column<long>(name: "score_ui6", type: "INTEGER", nullable: false),
                    scorebi1 = table.Column<long>(name: "score_bi1", type: "INTEGER", nullable: false),
                    lastplaytenpoid = table.Column<string>(name: "last_play_tenpo_id", type: "TEXT", nullable: true),
                    fcol1 = table.Column<long>(type: "INTEGER", nullable: false),
                    fcol2 = table.Column<long>(type: "INTEGER", nullable: false),
                    fcol3 = table.Column<long>(type: "INTEGER", nullable: false),
                    lastplaytime = table.Column<long>(name: "last_play_time", type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_detail", x => new { x.cardid, x.pcol1, x.pcol2, x.pcol3 });
                });

            migrationBuilder.CreateTable(
                name: "card_main",
                columns: table => new
                {
                    cardid = table.Column<long>(name: "card_id", type: "INTEGER", nullable: false),
                    playername = table.Column<string>(name: "player_name", type: "TEXT", nullable: false),
                    scorei1 = table.Column<long>(name: "score_i1", type: "INTEGER", nullable: false),
                    fcol1 = table.Column<long>(type: "INTEGER", nullable: false),
                    fcol2 = table.Column<long>(type: "INTEGER", nullable: false),
                    fcol3 = table.Column<long>(type: "INTEGER", nullable: false),
                    achievestatus = table.Column<string>(name: "achieve_status", type: "TEXT", nullable: false),
                    created = table.Column<string>(type: "TEXT", nullable: true),
                    modified = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_main", x => x.cardid);
                });

            migrationBuilder.CreateTable(
                name: "CardPlayCount",
                columns: table => new
                {
                    cardid = table.Column<long>(name: "card_id", type: "INTEGER", nullable: false),
                    playcount = table.Column<long>(name: "play_count", type: "INTEGER", nullable: false),
                    lastplayedtime = table.Column<long>(name: "last_played_time", type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardPlayCount", x => x.cardid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_bdata");

            migrationBuilder.DropTable(
                name: "card_detail");

            migrationBuilder.DropTable(
                name: "card_main");

            migrationBuilder.DropTable(
                name: "CardPlayCount");
        }
    }
}
