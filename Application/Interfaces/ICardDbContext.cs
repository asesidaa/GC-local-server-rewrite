using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface ICardDbContext
{
    public DbSet<CardBdatum> CardBdata { get; set; }

    public DbSet<CardDetail> CardDetails { get; set; } 

    public DbSet<CardMain> CardMains { get; set; } 

    public DbSet<CardPlayCount> CardPlayCounts { get; set; }
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    
    
}