using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractorInvoicing.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientNextInvoiceSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NextInvoiceSequence",
                table: "Clients",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextInvoiceSequence",
                table: "Clients");
        }
    }
}
