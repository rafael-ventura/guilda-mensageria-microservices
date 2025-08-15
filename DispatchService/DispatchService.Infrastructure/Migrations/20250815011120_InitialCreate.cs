using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatchService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Processado = table.Column<bool>(type: "bit", nullable: false),
                    TentativasProcessamento = table.Column<int>(type: "int", nullable: false),
                    ProximaTentativaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErroUltimoProcessamento = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Remetente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Destinatario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EnderecoEntrega = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recados", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CriadoEm",
                table: "OutboxMessages",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Processado",
                table: "OutboxMessages",
                column: "Processado");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Processado_ProximaTentativaEm_TentativasProcessamento",
                table: "OutboxMessages",
                columns: new[] { "Processado", "ProximaTentativaEm", "TentativasProcessamento" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProximaTentativaEm",
                table: "OutboxMessages",
                column: "ProximaTentativaEm");

            migrationBuilder.CreateIndex(
                name: "IX_Recados_CriadoEm",
                table: "Recados",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Recados_Destinatario",
                table: "Recados",
                column: "Destinatario");

            migrationBuilder.CreateIndex(
                name: "IX_Recados_Remetente",
                table: "Recados",
                column: "Remetente");

            migrationBuilder.CreateIndex(
                name: "IX_Recados_Status",
                table: "Recados",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "Recados");
        }
    }
}
