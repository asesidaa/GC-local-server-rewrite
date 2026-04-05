using Domain.Entities;

namespace Application.Interfaces;

public interface ICardDbContext
{
    public DbSet<CardBdatum> CardBdata { get; set; }

    public DbSet<CardDetail> CardDetails { get; set; } 

    public DbSet<CardMain> CardMains { get; set; } 

    public DbSet<CardPlayCount> CardPlayCounts { get; set; }
    
    public DbSet<PlayNumRank> PlayNumRanks { get; set; }
    
    public DbSet<GlobalScoreRank> GlobalScoreRanks { get; set; } 
    
    public DbSet<MonthlyScoreRank> MonthlyScoreRanks { get; set; } 
    
    public DbSet<ShopScoreRank> ShopScoreRanks { get; set; }

    public DbSet<DefaultUnlockState> DefaultUnlockStates { get; set; }

    public DbSet<PlayerUnlockState> PlayerUnlockStates { get; set; }

    public DbSet<PlayerCoin> PlayerCoins { get; set; }

    public DbSet<ShopItem> ShopItems { get; set; }

    public DbSet<CardAccessCode> CardAccessCodes { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    
    
}