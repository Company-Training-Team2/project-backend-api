using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <summary>
    /// Adds the SavedPaymentMethods table backing GET/POST/DELETE
    /// /api/payments/methods (audit Module 8 API contract — "Manage saved
    /// customer payment instruments").
    ///
    /// PRE-PUSH REVIEW NOTE: hand-written, same as 20260807000000_AddAIPlannerTables
    /// (no .NET SDK available in the environment that authored this change).
    /// Mirrors the AuditableEntity + single-FK-cascade pattern used by
    /// comparable tables in 20260805171711_InitialCreate.cs. No .Designer.cs
    /// and ApplicationDbContextModelSnapshot.cs was NOT updated for the same
    /// reason documented there.
    ///
    /// Before relying on this: run `dotnet ef migrations add AddSavedPaymentMethods`
    /// on a machine with the .NET SDK — if the model already matches this
    /// migration, EF will report "no changes"; otherwise it will generate a
    /// small corrective migration and regenerate a correct Designer.cs/snapshot.
    /// </summary>
    [Migration("20260808000000_AddSavedPaymentMethods")]
    public partial class AddSavedPaymentMethods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedPaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MaskedNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CardHolderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExpiryMonth = table.Column<int>(type: "int", nullable: true),
                    ExpiryYear = table.Column<int>(type: "int", nullable: true),
                    GatewayToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedPaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedPaymentMethods_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedPaymentMethods_CustomerId",
                table: "SavedPaymentMethods",
                column: "CustomerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SavedPaymentMethods");
        }
    }
}
