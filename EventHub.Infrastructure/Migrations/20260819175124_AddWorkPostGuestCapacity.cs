using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkPostGuestCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxGuests",
                table: "WorkPosts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinGuests",
                table: "WorkPosts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessLicensePath",
                table: "VendorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommercialRegistrationPath",
                table: "VendorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "VendorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdPath",
                table: "VendorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerUserId = table.Column<int>(type: "int", nullable: false),
                    VendorUserId = table.Column<int>(type: "int", nullable: false),
                    WorkPostId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_CustomerUserId",
                        column: x => x.CustomerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_VendorUserId",
                        column: x => x.VendorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_WorkPosts_WorkPostId",
                        column: x => x.WorkPostId,
                        principalTable: "WorkPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VendorProfileCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorProfileId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorProfileCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorProfileCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendorProfileCategories_VendorProfiles_VendorProfileId",
                        column: x => x.VendorProfileId,
                        principalTable: "VendorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    SenderUserId = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsReadByCustomer = table.Column<bool>(type: "bit", nullable: false),
                    IsReadByVendor = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationMessages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_ConversationId",
                table: "ConversationMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CustomerUserId",
                table: "Conversations",
                column: "CustomerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_VendorUserId",
                table: "Conversations",
                column: "VendorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_WorkPostId",
                table: "Conversations",
                column: "WorkPostId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfileCategories_CategoryId",
                table: "VendorProfileCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfileCategories_VendorProfileId_CategoryId",
                table: "VendorProfileCategories",
                columns: new[] { "VendorProfileId", "CategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationMessages");

            migrationBuilder.DropTable(
                name: "VendorProfileCategories");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropColumn(
                name: "MaxGuests",
                table: "WorkPosts");

            migrationBuilder.DropColumn(
                name: "MinGuests",
                table: "WorkPosts");

            migrationBuilder.DropColumn(
                name: "BusinessLicensePath",
                table: "VendorProfiles");

            migrationBuilder.DropColumn(
                name: "CommercialRegistrationPath",
                table: "VendorProfiles");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "VendorProfiles");

            migrationBuilder.DropColumn(
                name: "NationalIdPath",
                table: "VendorProfiles");
        }
    }
}
