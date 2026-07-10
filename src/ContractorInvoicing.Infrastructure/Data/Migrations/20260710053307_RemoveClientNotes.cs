using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractorInvoicing.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClientNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Clients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Clients",
                type: "TEXT",
                nullable: true);
        }
    }
}
