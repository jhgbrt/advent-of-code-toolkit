using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Net.Code.AdventOfCode.Toolkit.Migrations
{
    /// <inheritdoc />
    public partial class Results : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Day = table.Column<int>(type: "INTEGER", nullable: false),
                    Part1Status = table.Column<int>(name: "Part1_Status", type: "INTEGER", nullable: false),
                    Part1Value = table.Column<string>(name: "Part1_Value", type: "TEXT", nullable: false),
                    Part1Elapsed = table.Column<TimeSpan>(name: "Part1_Elapsed", type: "TEXT", nullable: false),
                    Part2Status = table.Column<int>(name: "Part2_Status", type: "INTEGER", nullable: false),
                    Part2Value = table.Column<string>(name: "Part2_Value", type: "TEXT", nullable: false),
                    Part2Elapsed = table.Column<TimeSpan>(name: "Part2_Elapsed", type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => new { x.Year, x.Day });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");
        }
    }
}
