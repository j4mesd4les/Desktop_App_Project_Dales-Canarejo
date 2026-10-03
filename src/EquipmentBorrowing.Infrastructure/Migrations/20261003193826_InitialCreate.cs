using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EquipmentBorrowing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsAllowedToBorrow = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Borrowings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    BorrowedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpectedReturnOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ReturnedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Borrowings", x => x.Id);
                    table.CheckConstraint("CK_Borrowings_DueAfterBorrowed", "ExpectedReturnOn > BorrowedOn");
                    table.CheckConstraint("CK_Borrowings_StatusMatchesReturnedOn", "(Status = 'Active' AND ReturnedOn IS NULL) OR (Status = 'Returned' AND ReturnedOn IS NOT NULL)");
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
                columns: new[] { "Id", "IsAvailable", "Name", "Type" },
                values: new object[,]
                {
                    { 1, true, "HDMI Projector", "Audio Visual" },
                    { 2, true, "Laptop", "Computer" },
                    { 3, true, "Extension Cord", "Accessory" },
                    { 4, true, "Speaker", "Audio Visual" },
                    { 5, false, "Laptop 02", "Computer" },
                    { 6, false, "Camera", "Photography" },
                    { 7, false, "Tripod", "Photography" },
                    { 8, false, "Microphone", "Audio Visual" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "IsAllowedToBorrow", "Name", "StudentNumber" },
                values: new object[,]
                {
                    { 1, true, "Dodong", "2023-0001" },
                    { 2, true, "Ana Reyes", "2023-0002" },
                    { 3, true, "Ben Santos", "2023-0003" },
                    { 4, false, "Carla Mendoza", "2023-0004" }
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
                name: "IX_Borrowings_EquipmentId",
                table: "Borrowings",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_StudentId_Status",
                table: "Borrowings",
                columns: new[] { "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Name",
                table: "Equipment",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Type",
                table: "Equipment",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumber",
                table: "Students",
                column: "StudentNumber",
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
