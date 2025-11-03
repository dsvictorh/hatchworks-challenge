using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CartonCaps.Referrals.Infrastructure.Persistence;

namespace CartonCaps.Referrals.Infrastructure.Migrations;

[DbContext(typeof(ReferralsDbContext))]
[Migration("20251103000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ReferralCodes",
            columns: table => new
            {
                Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReferralCodes", x => x.Code);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ReferralCodes_UserId",
            table: "ReferralCodes",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateTable(
            name: "Referrals",
            columns: table => new
            {
                Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ReferrerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                RefereeUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ReferralCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                InvitedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                RegisteredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Referrals", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Referrals_ReferralCode",
            table: "Referrals",
            column: "ReferralCode");

        migrationBuilder.CreateIndex(
            name: "IX_Referrals_ReferrerId_Status",
            table: "Referrals",
            columns: new[] { "ReferrerId", "Status" });

        // Lifecycle tables merged from second migration
        migrationBuilder.CreateTable(
            name: "ReferralEvents",
            columns: table => new
            {
                Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                EventType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                ReferralCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                EventId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                DeviceId = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReferralEvents", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ReferralEvents_EventId",
            table: "ReferralEvents",
            column: "EventId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ReferralEvents_ReferralCode_EventType",
            table: "ReferralEvents",
            columns: new[] { "ReferralCode", "EventType" });

        migrationBuilder.CreateTable(
            name: "ReferralLinks",
            columns: table => new
            {
                Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ReferralCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Url = table.Column<string>(type: "text", nullable: false),
                Channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                VendorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                MetadataJson = table.Column<string>(type: "text", nullable: true),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReferralLinks", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ReferralLinks_ReferralCode_Channel",
            table: "ReferralLinks",
            columns: new[] { "ReferralCode", "Channel" });

        migrationBuilder.CreateTable(
            name: "ReferralSessions",
            columns: table => new
            {
                SessionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ReferralCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReferralSessions", x => x.SessionId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ReferralSessions_ReferralCode_DeviceId",
            table: "ReferralSessions",
            columns: new[] { "ReferralCode", "DeviceId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ReferralSessions");
        migrationBuilder.DropTable(name: "ReferralLinks");
        migrationBuilder.DropTable(name: "ReferralEvents");
        migrationBuilder.DropTable(
            name: "Referrals");

        migrationBuilder.DropTable(
            name: "ReferralCodes");
    }
}
