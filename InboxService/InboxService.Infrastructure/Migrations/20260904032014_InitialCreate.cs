using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InboxService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItensTimeline",
                columns: table => new
                {
                    RecadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Remetente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Destinatario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EnderecoEntrega = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EntregueEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoFalha = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensTimeline", x => x.RecadoId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItensTimeline_Destinatario",
                table: "ItensTimeline",
                column: "Destinatario");

            migrationBuilder.CreateIndex(
                name: "IX_ItensTimeline_Status",
                table: "ItensTimeline",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItensTimeline");
        }
    }
}
