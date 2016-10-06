using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EmailService.Web.Migrations
{
    public partial class AddingLayout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Layouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ApplicationId = table.Column<Guid>(nullable: false),
                    BodyHtml = table.Column<string>(nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(rowVersion: true, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Layouts_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "LayoutId",
                table: "Templates",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Layouts_ApplicationId",
                table: "Layouts",
                column: "ApplicationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LayoutId",
                table: "Templates");

            migrationBuilder.DropTable(
                name: "Layouts");
        }
    }
}
