using CartonCaps.Referrals.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CartonCaps.Referrals.Infrastructure.Persistence;

public class ReferralsDbContext : DbContext
{
    public ReferralsDbContext(DbContextOptions<ReferralsDbContext> options) : base(options)
    {
    }

    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<ReferralCode> ReferralCodes => Set<ReferralCode>();
    public DbSet<ReferralEvent> ReferralEvents => Set<ReferralEvent>();
    public DbSet<ReferralSession> ReferralSessions => Set<ReferralSession>();
    public DbSet<ReferralLink> ReferralLinks => Set<ReferralLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Referral>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).HasMaxLength(64);
            b.Property(r => r.ReferrerId).IsRequired().HasMaxLength(64);
            b.Property(r => r.RefereeUserId).HasMaxLength(64);
            b.Property(r => r.ReferralCode).IsRequired().HasMaxLength(20);
            b.Property(r => r.Status).IsRequired().HasMaxLength(32);
            b.Property(r => r.Channel).HasMaxLength(32);
            b.HasIndex(r => new { r.ReferrerId, r.Status });
            b.HasIndex(r => r.ReferralCode);
        });

        modelBuilder.Entity<ReferralCode>(b =>
        {
            b.HasKey(rc => rc.Code);
            b.Property(rc => rc.Code).HasMaxLength(20);
            b.Property(rc => rc.UserId).IsRequired().HasMaxLength(64);
            b.Property(rc => rc.Status).IsRequired().HasMaxLength(32);
            b.HasIndex(rc => rc.UserId).IsUnique();
        });

        modelBuilder.Entity<ReferralEvent>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).HasMaxLength(64);
            b.Property(e => e.EventType).IsRequired().HasMaxLength(32);
            b.Property(e => e.ReferralCode).IsRequired().HasMaxLength(20);
            b.Property(e => e.EventId).HasMaxLength(64);
            b.HasIndex(e => e.EventId).IsUnique();
            b.HasIndex(e => new { e.ReferralCode, e.EventType });
        });

        modelBuilder.Entity<ReferralSession>(b =>
        {
            b.HasKey(s => s.SessionId);
            b.Property(s => s.SessionId).HasMaxLength(64);
            b.Property(s => s.ReferralCode).IsRequired().HasMaxLength(20);
            b.Property(s => s.DeviceId).IsRequired().HasMaxLength(100);
            b.Property(s => s.Status).IsRequired().HasMaxLength(32);
            b.HasIndex(s => new { s.ReferralCode, s.DeviceId }).IsUnique();
        });

        modelBuilder.Entity<ReferralLink>(b =>
        {
            b.HasKey(l => l.Id);
            b.Property(l => l.Id).HasMaxLength(64);
            b.Property(l => l.Url).IsRequired();
            b.Property(l => l.ReferralCode).IsRequired().HasMaxLength(20);
            b.Property(l => l.Channel).HasMaxLength(32);
            b.Property(l => l.VendorId).HasMaxLength(64);
            b.HasIndex(l => new { l.ReferralCode, l.Channel });
        });
    }
}
