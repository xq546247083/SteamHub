using Microsoft.EntityFrameworkCore;
using SteamHub.Entities;

namespace SteamHub;

/// <summary>
/// SteamHub 数据库上下文
/// </summary>
public class SteamHubDbContext : DbContext
{
    /// <summary>
    /// Steam 账号表
    /// </summary>
    public DbSet<SteamAccount> SteamAccounts
    {
        get; set;
    }

    /// <summary>
    /// 配置表
    /// </summary>
    public DbSet<Setting> Settings
    {
        get; set;
    }

    /// <summary>
    /// Steam 游戏表
    /// </summary>
    public DbSet<SteamGame> SteamGames
    {
        get; set;
    }

    /// <summary>
    /// Steam 账号与游戏关系表
    /// </summary>
    public DbSet<SteamAccountGame> SteamAccountGames
    {
        get; set;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.DbFileName);
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Steam 账号配置
        modelBuilder.Entity<SteamAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Account).IsUnique();
            entity.Property(e => e.Account).IsRequired().HasMaxLength(64);
            entity.Property(e => e.SteamId).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.Password).HasMaxLength(64);
        });

        // 配置表配置
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Value).IsRequired();
        });

        // Steam 游戏配置
        modelBuilder.Entity<SteamGame>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AppId);
            entity.Property(e => e.AppId).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.CurrentSelectAccountSteamId).HasMaxLength(64);
        });

        // Steam 账号与游戏关系配置
        modelBuilder.Entity<SteamAccountGame>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SteamId, e.AppId });
            entity.Property(e => e.SteamId).IsRequired().HasMaxLength(64);
            entity.Property(e => e.AppId).IsRequired().HasMaxLength(64);
        });
    }
}