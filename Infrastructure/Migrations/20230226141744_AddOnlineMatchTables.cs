using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOnlineMatchTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "last_play_time",
                table: "card_detail",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "OnlineMatches",
                columns: table => new
                {
                    MatchId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsOpen = table.Column<bool>(type: "INTEGER", nullable: false),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineMatches", x => x.MatchId);
                });

            migrationBuilder.CreateTable(
                name: "OnlineMatchEntries",
                columns: table => new
                {
                    MatchId = table.Column<long>(type: "INTEGER", nullable: false),
                    EntryId = table.Column<long>(type: "INTEGER", nullable: false),
                    MachineId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<long>(type: "INTEGER", nullable: false),
                    CardId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", nullable: false),
                    AvatarId = table.Column<long>(type: "INTEGER", nullable: false),
                    TitleId = table.Column<long>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<long>(type: "INTEGER", nullable: false),
                    GroupId = table.Column<long>(type: "INTEGER", nullable: false),
                    TenpoId = table.Column<long>(type: "INTEGER", nullable: false),
                    TenpoName = table.Column<string>(type: "TEXT", nullable: false),
                    PrefId = table.Column<long>(type: "INTEGER", nullable: false),
                    Pref = table.Column<string>(type: "TEXT", nullable: false),
                    MessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchTimeout = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchWaitTime = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchRemainingTime = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineMatchEntries", x => new { x.MatchId, x.EntryId });
                    table.ForeignKey(
                        name: "FK_OnlineMatchEntries_OnlineMatches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "OnlineMatches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnlineMatchEntries");

            migrationBuilder.DropTable(
                name: "OnlineMatches");

            migrationBuilder.AlterColumn<long>(
                name: "last_play_time",
                table: "card_detail",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
