using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Net.Code.AdventOfCode.Toolkit.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Puzzles",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false, computedColumnSql: "Key/100"),
                    Day = table.Column<int>(type: "INTEGER", nullable: false, computedColumnSql: "Key%100"),
                    Input = table.Column<string>(type: "TEXT", nullable: false),
                    Answerpart1 = table.Column<string>(name: "Answer_part1", type: "TEXT", nullable: false),
                    Answerpart2 = table.Column<string>(name: "Answer_part2", type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puzzles", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false, computedColumnSql: "Key/100"),
                    Day = table.Column<int>(type: "INTEGER", nullable: false, computedColumnSql: "Key%100"),
                    Part1Status = table.Column<int>(name: "Part1_Status", type: "INTEGER", nullable: false),
                    Part1Value = table.Column<string>(name: "Part1_Value", type: "TEXT", nullable: false),
                    Part1Elapsed = table.Column<long>(name: "Part1_Elapsed", type: "INTEGER", nullable: false),
                    Part2Status = table.Column<int>(name: "Part2_Status", type: "INTEGER", nullable: false),
                    Part2Value = table.Column<string>(name: "Part2_Value", type: "TEXT", nullable: false),
                    Part2Elapsed = table.Column<long>(name: "Part2_Elapsed", type: "INTEGER", nullable: false),
                    Elapsed = table.Column<long>(type: "INTEGER", nullable: false, computedColumnSql: "Part1_Elapsed + Part2_Elapsed")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Puzzles_Year_Day",
                table: "Puzzles",
                columns: new[] { "Year", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_Results_Year_Day",
                table: "Results",
                columns: new[] { "Year", "Day" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Puzzles");

            migrationBuilder.DropTable(
                name: "Results");
        }
    }
}
