using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitLoggingDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActionType = table.Column<byte>(type: "smallint", nullable: false),
                    ActorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ActorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActorName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ActorAvatarUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    TargetType = table.Column<byte>(type: "smallint", nullable: false),
                    TargetId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TargetCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ActivityLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    Field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<byte>(type: "smallint", nullable: true),
                    OldValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OldCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OldId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NewCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NewId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogChanges_ActivityLogs_ActivityLogId",
                        column: x => x.ActivityLogId,
                        principalTable: "ActivityLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ActorId",
                table: "ActivityLogs",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_TargetId",
                table: "ActivityLogs",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Timestamp",
                table: "ActivityLogs",
                column: "Timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_LogChanges_ActivityLogId",
                table: "LogChanges",
                column: "ActivityLogId");

            migrationBuilder.CreateIndex(
                name: "IX_LogChanges_Field_NewId",
                table: "LogChanges",
                columns: new[] { "Field", "NewId" },
                filter: "\"NewId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LogChanges_Field_OldId",
                table: "LogChanges",
                columns: new[] { "Field", "OldId" },
                filter: "\"OldId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogChanges");

            migrationBuilder.DropTable(
                name: "ActivityLogs");
        }
    }
}
