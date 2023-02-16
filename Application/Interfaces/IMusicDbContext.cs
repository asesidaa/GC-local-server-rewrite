using Domain.Entities;

namespace Application.Interfaces;

public interface IMusicDbContext
{
    public  DbSet<MusicAou> MusicAous { get; set; }

    public  DbSet<MusicExtra> MusicExtras { get; set; } 

    public  DbSet<MusicUnlock> MusicUnlocks { get; set; } 
}