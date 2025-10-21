using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendVacancies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Vacancies",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Vacancies",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Vacancies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_Status",
                table: "Vacancies",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vacancies_Status",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Vacancies");
        }
    }
}
