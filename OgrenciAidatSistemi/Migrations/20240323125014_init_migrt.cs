using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciAidatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class init_migrt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilePath",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Extension = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilePath", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PaymentPeriods",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Occurence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentPeriods", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    EmailAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(
                        type: "TEXT",
                        maxLength: 13,
                        nullable: false
                    ),
                    SchoolAdmin_SchoolId = table.Column<int>(type: "INTEGER", nullable: true),
                    SiteAdminId = table.Column<int>(type: "INTEGER", nullable: true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: true),
                    SchoolId = table.Column<int>(type: "INTEGER", nullable: true),
                    GradLevel = table.Column<int>(type: "INTEGER", nullable: true),
                    IsGraduated = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Schools_SchoolAdmin_SchoolId",
                        column: x => x.SchoolAdmin_SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Users_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentPeriodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    PaymentPeriodId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaymentType = table.Column<string>(
                        type: "TEXT",
                        maxLength: 21,
                        nullable: false
                    ),
                    BankTransferPayment_BankName = table.Column<string>(
                        type: "TEXT",
                        nullable: true
                    ),
                    AccountNumber = table.Column<string>(type: "TEXT", nullable: true),
                    BankTransferPayment_BranchCode = table.Column<string>(
                        type: "TEXT",
                        nullable: true
                    ),
                    CashierName = table.Column<string>(type: "TEXT", nullable: true),
                    ReceiptId = table.Column<int>(type: "INTEGER", nullable: true),
                    CheckNumber = table.Column<string>(type: "TEXT", nullable: true),
                    BankName = table.Column<string>(type: "TEXT", nullable: true),
                    BranchCode = table.Column<string>(type: "TEXT", nullable: true),
                    CreditCardPayment_CardNumber = table.Column<string>(
                        type: "TEXT",
                        nullable: true
                    ),
                    CreditCardPayment_CardHolderName = table.Column<string>(
                        type: "TEXT",
                        nullable: true
                    ),
                    CreditCardPayment_ExpiryDate = table.Column<string>(
                        type: "TEXT",
                        nullable: true
                    ),
                    CreditCardPayment_CVC = table.Column<string>(type: "TEXT", nullable: true),
                    CardNumber = table.Column<string>(type: "TEXT", nullable: true),
                    CardHolderName = table.Column<string>(type: "TEXT", nullable: true),
                    ExpiryDate = table.Column<string>(type: "TEXT", nullable: true),
                    CVC = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_FilePath_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "FilePath",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Payments_PaymentPeriods_PaymentPeriodeId",
                        column: x => x.PaymentPeriodeId,
                        principalTable: "PaymentPeriods",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_Payments_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentPeriodeId",
                table: "Payments",
                column: "PaymentPeriodeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReceiptId",
                table: "Payments",
                column: "ReceiptId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId",
                table: "Payments",
                column: "StudentId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_SchoolAdmin_SchoolId",
                table: "Users",
                column: "SchoolAdmin_SchoolId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_SchoolId",
                table: "Users",
                column: "SchoolId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Payments");

            migrationBuilder.DropTable(name: "FilePath");

            migrationBuilder.DropTable(name: "PaymentPeriods");

            migrationBuilder.DropTable(name: "Users");

            migrationBuilder.DropTable(name: "Schools");
        }
    }
}
