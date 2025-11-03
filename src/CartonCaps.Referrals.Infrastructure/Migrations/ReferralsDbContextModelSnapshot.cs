using CartonCaps.Referrals.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CartonCaps.Referrals.Infrastructure.Migrations;

[DbContext(typeof(ReferralsDbContext))]
public class ReferralsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "9.0.1");

        modelBuilder.Entity("CartonCaps.Referrals.Core.Domain.Entities.Referral", b =>
        {
            b.Property<string>("Id")
                .HasMaxLength(64)
                .HasColumnType("character varying(64)");

            b.Property<string>("Channel")
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.Property<DateTimeOffset>("InvitedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("RegisteredAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("RefereeUserId")
                .HasMaxLength(64)
                .HasColumnType("character varying(64)");

            b.Property<string>("ReferralCode")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)");

            b.Property<string>("ReferrerId")
                .IsRequired()
                .HasMaxLength(64)
                .HasColumnType("character varying(64)");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.HasKey("Id");

            b.HasIndex("ReferralCode");

            b.HasIndex("ReferrerId", "Status");

            b.ToTable("Referrals");
        });

        modelBuilder.Entity("CartonCaps.Referrals.Core.Domain.Entities.ReferralCode", b =>
        {
            b.Property<string>("Code")
                .HasMaxLength(20)
                .HasColumnType("character varying(20)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.Property<string>("UserId")
                .IsRequired()
                .HasMaxLength(64)
                .HasColumnType("character varying(64)");

            b.HasKey("Code");

            b.HasIndex("UserId")
                .IsUnique();

            b.ToTable("ReferralCodes");
        });

        modelBuilder.Entity("CartonCaps.Referrals.Core.Domain.Entities.ReferralEvent", b =>
        {
            b.Property<string>("Id")
                .HasMaxLength(64)
                .HasColumnType("character varying(64)");

            b.Property<string>("DeviceId")
                .HasColumnType("text");

            b.Property<string>("EventId")
                .HasMaxLength(64)
                .HasColumnType("character varying(64)");

            b.Property<string>("EventType")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.Property<string>("ReferralCode")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)");

            b.Property<DateTimeOffset>("Timestamp")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("EventId")
                .IsUnique();

            b.HasIndex("ReferralCode", "EventType");

            b.ToTable("ReferralEvents");
        });

        modelBuilder.Entity("CartonCaps.Referrals.Core.Domain.Entities.ReferralLink", b =>
        {
            b.Property<string>("Id")
                .HasMaxLength(64)
                .HasColumnType("character varying(64)");

            b.Property<string>("Channel")
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("ExpiresAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("MetadataJson")
                .HasColumnType("text");

            b.Property<string>("ReferralCode")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)");

            b.Property<string>("Url")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("VendorId")
                .HasMaxLength(64)
                .HasColumnType("character varying(64)");

            b.HasKey("Id");

            b.HasIndex("ReferralCode", "Channel");

            b.ToTable("ReferralLinks");
        });

        modelBuilder.Entity("CartonCaps.Referrals.Core.Domain.Entities.ReferralSession", b =>
        {
            b.Property<string>("SessionId")
                .HasMaxLength(64)
                .HasColumnType("character varying(64)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("DeviceId")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<DateTimeOffset>("ExpiresAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("ReferralCode")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.HasKey("SessionId");

            b.HasIndex("ReferralCode", "DeviceId")
                .IsUnique();

            b.ToTable("ReferralSessions");
        });
#pragma warning restore 612, 618
    }
}
