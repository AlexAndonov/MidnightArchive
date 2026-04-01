using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidnightArchive.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Event unique identifier"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Event title"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, comment: "Event description"),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Location of the event. It is not mandatory, because event can be online!"),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: false, comment: "Unique identifier of the event's creator"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Event start date"),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Event end date"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, comment: "Indicated whether event is soft-deleted")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Event class");

            migrationBuilder.CreateTable(
                name: "EventParticipants",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Unique identifier of the event"),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", nullable: false, comment: "Unique identifier for the user"),
                    JoinedOn = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Indicates when the user joined the event")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventParticipants", x => new { x.EventId, x.ParticipantId });
                    table.ForeignKey(
                        name: "FK_EventParticipants_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventParticipants_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Join table for Events and Users");

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipants_ParticipantId",
                table: "EventParticipants",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatorId",
                table: "Events",
                column: "CreatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventParticipants");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
