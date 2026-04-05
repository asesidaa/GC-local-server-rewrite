using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence;

public partial class CardDbContext : DbContext, ICardDbContext
{
    public CardDbContext()
    {
    }

    public CardDbContext(DbContextOptions<CardDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CardBdatum> CardBdata { get; set; } = null!;

    public virtual DbSet<CardDetail> CardDetails { get; set; } = null!;

    public virtual DbSet<CardMain> CardMains { get; set; } = null!;

    public virtual DbSet<CardPlayCount> CardPlayCounts { get; set; } = null!;

    public virtual DbSet<PlayNumRank> PlayNumRanks { get; set; } = null!;

    public virtual DbSet<GlobalScoreRank> GlobalScoreRanks { get; set; } = null!;
    
    public virtual DbSet<MonthlyScoreRank> MonthlyScoreRanks { get; set; } = null!;
    
    public virtual DbSet<ShopScoreRank> ShopScoreRanks { get; set; } = null!;

    public virtual DbSet<DefaultUnlockState> DefaultUnlockStates { get; set; } = null!;

    public virtual DbSet<PlayerUnlockState> PlayerUnlockStates { get; set; } = null!;

    public virtual DbSet<PlayerCoin> PlayerCoins { get; set; } = null!;

    public virtual DbSet<ShopItem> ShopItems { get; set; } = null!;

    public virtual DbSet<CardAccessCode> CardAccessCodes { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var defaultDb = Path.Combine(PathHelper.DatabasePath, "card.db3");
        optionsBuilder.UseSqlite($"Data Source={defaultDb}");
    }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CardBdatum>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.ToTable("card_bdata");

            entity.Property(e => e.CardId)
                .ValueGeneratedNever()
                .HasColumnName("card_id");
            entity.Property(e => e.Bdata).HasColumnName("bdata");
            entity.Property(e => e.BdataSize).HasColumnName("bdata_size");
        });

        modelBuilder.Entity<CardDetail>(entity =>
        {
            entity.HasKey(e => new { e.CardId, e.Pcol1, e.Pcol2, e.Pcol3 });

            entity.ToTable("card_detail");

            entity.Property(e => e.CardId).HasColumnName("card_id");
            entity.Property(e => e.Pcol1).HasColumnName("pcol1");
            entity.Property(e => e.Pcol2).HasColumnName("pcol2");
            entity.Property(e => e.Pcol3).HasColumnName("pcol3");
            entity.Property(e => e.Fcol1).HasColumnName("fcol1");
            entity.Property(e => e.Fcol2).HasColumnName("fcol2");
            entity.Property(e => e.Fcol3).HasColumnName("fcol3");
            entity.Property(e => e.LastPlayTenpoId).HasColumnName("last_play_tenpo_id").IsRequired(false);
            entity.Property(e => e.LastPlayTime).HasColumnName("last_play_time")
                .HasConversion<DateTimeToTicksConverter>().IsRequired(false);
            entity.Property(e => e.ScoreBi1).HasColumnName("score_bi1");
            entity.Property(e => e.ScoreI1).HasColumnName("score_i1");
            entity.Property(e => e.ScoreUi1).HasColumnName("score_ui1");
            entity.Property(e => e.ScoreUi2).HasColumnName("score_ui2");
            entity.Property(e => e.ScoreUi3).HasColumnName("score_ui3");
            entity.Property(e => e.ScoreUi4).HasColumnName("score_ui4");
            entity.Property(e => e.ScoreUi5).HasColumnName("score_ui5");
            entity.Property(e => e.ScoreUi6).HasColumnName("score_ui6");
        });

        modelBuilder.Entity<CardMain>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.ToTable("card_main");

            entity.Property(e => e.CardId)
                .ValueGeneratedNever()
                .HasColumnName("card_id");
            entity.Property(e => e.AchieveStatus).HasColumnName("achieve_status");
            entity.Property(e => e.Created).HasColumnName("created").IsRequired(false);
            entity.Property(e => e.Fcol1).HasColumnName("fcol1");
            entity.Property(e => e.Fcol2).HasColumnName("fcol2");
            entity.Property(e => e.Fcol3).HasColumnName("fcol3");
            entity.Property(e => e.Modified).HasColumnName("modified").IsRequired(false);
            entity.Property(e => e.PlayerName).HasColumnName("player_name");
            entity.Property(e => e.ScoreI1).HasColumnName("score_i1");
        });

        modelBuilder.Entity<CardPlayCount>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.ToTable("CardPlayCount");

            entity.Property(e => e.CardId)
                .ValueGeneratedNever()
                .HasColumnName("card_id");
            entity.Property(e => e.LastPlayedTime).HasColumnName("last_played_time")
                .HasConversion<DateTimeToTicksConverter>();
            entity.Property(e => e.PlayCount).HasColumnName("play_count");
        });

        modelBuilder.Entity<PlayNumRank>(entity =>
        {
            entity.HasKey(e => e.MusicId);

            entity.ToTable("PlayNumRank");

            entity.Property(e => e.MusicId).ValueGeneratedNever();
            entity.Property(e => e.PlayCount);
            entity.Property(e => e.Artist);
            entity.Property(e => e.Title);
            entity.Property(e => e.Rank);
            entity.Property(e => e.Rank2);
            entity.Property(e => e.PrevRank);
            entity.Property(e => e.PrevRank2);
        });

        modelBuilder.Entity<GlobalScoreRank>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.ToTable("GlobalScoreRank");

            entity.Property(e => e.CardId).ValueGeneratedNever();
            entity.Property(e => e.Fcol1);
            entity.Property(e => e.AvatarId);
            entity.Property(e => e.Title);
            entity.Property(e => e.TitleId);
            entity.Property(e => e.Rank);
            entity.Property(e => e.AreaId);
            entity.Property(e => e.Area);
            entity.Property(e => e.LastPlayTenpoId);
            entity.Property(e => e.TenpoName);
            entity.Property(e => e.PrefId);
            entity.Property(e => e.Pref);
            entity.Property(e => e.TotalScore);
            entity.Property(e => e.PlayerName);
        });
        
        modelBuilder.Entity<MonthlyScoreRank>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.ToTable("MonthlyScoreRank");

            entity.Property(e => e.CardId).ValueGeneratedNever();
            entity.Property(e => e.Fcol1);
            entity.Property(e => e.AvatarId);
            entity.Property(e => e.Title);
            entity.Property(e => e.TitleId);
            entity.Property(e => e.Rank);
            entity.Property(e => e.AreaId);
            entity.Property(e => e.Area);
            entity.Property(e => e.LastPlayTenpoId);
            entity.Property(e => e.TenpoName);
            entity.Property(e => e.PrefId);
            entity.Property(e => e.Pref);
            entity.Property(e => e.TotalScore);
            entity.Property(e => e.PlayerName);
        });
        
        modelBuilder.Entity<ShopScoreRank>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.ToTable("ShopScoreRank");

            entity.Property(e => e.CardId).ValueGeneratedNever();
            entity.Property(e => e.Fcol1);
            entity.Property(e => e.AvatarId);
            entity.Property(e => e.Title);
            entity.Property(e => e.TitleId);
            entity.Property(e => e.Rank);
            entity.Property(e => e.AreaId);
            entity.Property(e => e.Area);
            entity.Property(e => e.LastPlayTenpoId);
            entity.Property(e => e.TenpoName);
            entity.Property(e => e.PrefId);
            entity.Property(e => e.Pref);
            entity.Property(e => e.TotalScore);
            entity.Property(e => e.PlayerName);
        });

        modelBuilder.Entity<DefaultUnlockState>(entity =>
        {
            entity.HasKey(e => e.ItemType);

            entity.ToTable("default_unlock_state");

            entity.Property(e => e.ItemType)
                .ValueGeneratedNever()
                .HasColumnName("item_type");
            entity.Property(e => e.UnlockedBitset)
                .HasColumnName("unlocked_bitset")
                .HasDefaultValue("[]");
        });

        modelBuilder.Entity<PlayerUnlockState>(entity =>
        {
            entity.HasKey(e => new { e.CardId, e.ItemType });

            entity.ToTable("player_unlock_state");

            entity.Property(e => e.CardId).HasColumnName("card_id");
            entity.Property(e => e.ItemType).HasColumnName("item_type");
            entity.Property(e => e.UnlockedBitset)
                .HasColumnName("unlocked_bitset")
                .HasDefaultValue("[]");
        });

        modelBuilder.Entity<PlayerCoin>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.ToTable("player_coin");

            entity.Property(e => e.CardId)
                .ValueGeneratedNever()
                .HasColumnName("card_id");
            entity.Property(e => e.CurrentCoins).HasColumnName("current_coins");
            entity.Property(e => e.TotalCoins).HasColumnName("total_coins");
            entity.Property(e => e.MonthlyCoins).HasColumnName("monthly_coins");
        });

        modelBuilder.Entity<ShopItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("shop_item");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.ItemType).HasColumnName("item_type");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.CoinCost).HasColumnName("coin_cost");

            entity.HasIndex(e => new { e.ItemType, e.ItemId }).IsUnique();
        });

        modelBuilder.Entity<CardAccessCode>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.ToTable("card_access_code");

            entity.Property(e => e.CardId)
                .ValueGeneratedNever()
                .HasColumnName("card_id");
            entity.Property(e => e.HashedCode).HasColumnName("hashed_code");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
