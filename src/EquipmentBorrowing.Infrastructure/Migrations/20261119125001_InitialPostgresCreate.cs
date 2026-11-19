using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EquipmentBorrowing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EquipmentNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EquipmentNumberNormalized = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentNumberNormalized = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Contact = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    IsAllowedToBorrow = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Borrowings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    BorrowedOn = table.Column<DateTime>(type: "date", nullable: false),
                    ExpectedReturnOn = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReturnedOn = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Borrowings", x => x.Id);
                    table.CheckConstraint("CK_Borrowings_DueAfterBorrowed", "\"ExpectedReturnOn\" > \"BorrowedOn\"");
                    table.CheckConstraint("CK_Borrowings_StatusMatchesReturnedOn", "(\"Status\" = 'Active' AND \"ReturnedOn\" IS NULL) OR (\"Status\" = 'Returned' AND \"ReturnedOn\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Borrowings_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Borrowings_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Equipment",
                columns: new[] { "Id", "Description", "EquipmentNumber", "EquipmentNumberNormalized", "IsAvailable", "Name", "Type" },
                values: new object[,]
                {
                    { 1, null, "EQ-001", "EQ-001", true, "HDMI Projector", "Audio Visual" },
                    { 2, null, "EQ-002", "EQ-002", true, "Laptop", "Computer" },
                    { 3, null, "EQ-003", "EQ-003", true, "Extension Cord", "Accessory" },
                    { 4, null, "EQ-004", "EQ-004", true, "Speaker", "Audio Visual" },
                    { 5, null, "EQ-005", "EQ-005", false, "Laptop 02", "Computer" },
                    { 6, null, "EQ-006", "EQ-006", false, "Camera", "Photography" },
                    { 7, null, "EQ-007", "EQ-007", false, "Tripod", "Photography" },
                    { 8, null, "EQ-008", "EQ-008", false, "Microphone", "Audio Visual" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "Contact", "IsAllowedToBorrow", "Name", "StudentNumber", "StudentNumberNormalized" },
                values: new object[,]
                {
                    { 1, null, true, "Dodong", "2023-0001", "2023-0001" },
                    { 2, null, true, "Ana Reyes", "2023-0002", "2023-0002" },
                    { 3, null, true, "Ben Santos", "2023-0003", "2023-0003" },
                    { 4, null, false, "Carla Mendoza", "2023-0004", "2023-0004" }
                });

            migrationBuilder.InsertData(
                table: "Borrowings",
                columns: new[] { "Id", "BorrowedOn", "EquipmentId", "ExpectedReturnOn", "ReturnedOn", "Status", "StudentId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Active", 2 },
                    { 2, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Active", 3 },
                    { 3, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Active", 3 },
                    { 4, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Active", 3 },
                    { 5, new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Returned", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_StudentId_Status",
                table: "Borrowings",
                columns: new[] { "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_Borrowings_Equipment_Active",
                table: "Borrowings",
                column: "EquipmentId",
                unique: true,
                filter: "\"Status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EquipmentNumberNormalized",
                table: "Equipment",
                column: "EquipmentNumberNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Type",
                table: "Equipment",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumberNormalized",
                table: "Students",
                column: "StudentNumberNormalized",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Borrowings");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
