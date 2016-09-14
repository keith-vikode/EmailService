using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EmailService.Web.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ConcurrencyToken = table.Column<byte[]>(rowVersion: true, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    PrimaryApiKey = table.Column<byte[]>(nullable: true),
                    SecondaryApiKey = table.Column<byte[]>(nullable: true),
                    SenderAddress = table.Column<string>(maxLength: 255, nullable: false),
                    SenderName = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transports",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ConcurrencyToken = table.Column<byte[]>(rowVersion: true, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    Hostname = table.Column<string>(maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Password = table.Column<string>(maxLength: 255, nullable: true),
                    PortNum = table.Column<short>(nullable: true),
                    SenderAddress = table.Column<string>(maxLength: 255, nullable: false),
                    SenderName = table.Column<string>(maxLength: 50, nullable: true),
                    Type = table.Column<int>(nullable: false),
                    UseSSL = table.Column<bool>(nullable: false),
                    Username = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ApplicationId = table.Column<Guid>(nullable: false),
                    BodyTemplate = table.Column<string>(nullable: false),
                    ConcurrencyToken = table.Column<byte[]>(rowVersion: true, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    SampleData = table.Column<string>(nullable: true),
                    SubjectTemplate = table.Column<string>(maxLength: 255, nullable: false),
                    UseHtml = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Templates_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationTransport",
                columns: table => new
                {
                    ApplicationId = table.Column<Guid>(nullable: false),
                    TransportId = table.Column<Guid>(nullable: false),
                    Priority = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationTransport", x => new { x.ApplicationId, x.TransportId });
                    table.ForeignKey(
                        name: "FK_ApplicationTransport_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationTransport_Transports_TransportId",
                        column: x => x.TransportId,
                        principalTable: "Transports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Translations",
                columns: table => new
                {
                    TemplateId = table.Column<Guid>(nullable: false),
                    Language = table.Column<string>(maxLength: 10, nullable: false),
                    BodyTemplate = table.Column<string>(nullable: false),
                    ConcurrencyToken = table.Column<byte[]>(rowVersion: true, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    SubjectTemplate = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => new { x.TemplateId, x.Language });
                    table.ForeignKey(
                        name: "FK_Translations_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Name",
                table: "Applications",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationTransport_ApplicationId",
                table: "ApplicationTransport",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationTransport_TransportId",
                table: "ApplicationTransport",
                column: "TransportId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_ApplicationId",
                table: "Templates",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_ApplicationId_Name",
                table: "Templates",
                columns: new[] { "ApplicationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translations_TemplateId",
                table: "Translations",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Transports_Name",
                table: "Transports",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationTransport");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropTable(
                name: "Transports");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "Applications");
        }
    }
}
