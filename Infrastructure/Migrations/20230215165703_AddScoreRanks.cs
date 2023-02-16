using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreRanks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalScoreRank",
                columns: table => new
                {
                    CardId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", nullable: false),
                    Rank = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalScore = table.Column<long>(type: "INTEGER", nullable: false),
                    AvatarId = table.Column<int>(type: "INTEGER", nullable: false),
                    TitleId = table.Column<long>(type: "INTEGER", nullable: false),
                    Fcol1 = table.Column<long>(type: "INTEGER", nullable: false),
                    PrefId = table.Column<int>(type: "INTEGER", nullable: false),
                    Pref = table.Column<string>(type: "TEXT", nullable: false),
                    AreaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Area = table.Column<string>(type: "TEXT", nullable: false),
                    LastPlayTenpoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenpoName = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalScoreRank", x => x.CardId);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyScoreRank",
                columns: table => new
                {
                    CardId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", nullable: false),
                    Rank = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalScore = table.Column<long>(type: "INTEGER", nullable: false),
                    AvatarId = table.Column<int>(type: "INTEGER", nullable: false),
                    TitleId = table.Column<long>(type: "INTEGER", nullable: false),
                    Fcol1 = table.Column<long>(type: "INTEGER", nullable: false),
                    PrefId = table.Column<int>(type: "INTEGER", nullable: false),
                    Pref = table.Column<string>(type: "TEXT", nullable: false),
                    AreaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Area = table.Column<string>(type: "TEXT", nullable: false),
                    LastPlayTenpoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenpoName = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyScoreRank", x => x.CardId);
                });

            migrationBuilder.CreateTable(
                name: "ShopScoreRank",
                columns: table => new
                {
                    CardId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", nullable: false),
                    Rank = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalScore = table.Column<long>(type: "INTEGER", nullable: false),
                    AvatarId = table.Column<int>(type: "INTEGER", nullable: false),
                    TitleId = table.Column<long>(type: "INTEGER", nullable: false),
                    Fcol1 = table.Column<long>(type: "INTEGER", nullable: false),
                    PrefId = table.Column<int>(type: "INTEGER", nullable: false),
                    Pref = table.Column<string>(type: "TEXT", nullable: false),
                    AreaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Area = table.Column<string>(type: "TEXT", nullable: false),
                    LastPlayTenpoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenpoName = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopScoreRank", x => x.CardId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalScoreRank");

            migrationBuilder.DropTable(
                name: "MonthlyScoreRank");

            migrationBuilder.DropTable(
                name: "ShopScoreRank");
        }
    }
}
