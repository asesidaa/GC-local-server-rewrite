using SQLite.Net2;

namespace GCLocalServerRewrite.models;

public class CardPlayCount
{
    [PrimaryKey]
    [Column("card_id")]
    public long CardId { get; set; }
    
    [Column("play_count")]
    public int PlayCount { get; set; }
    
    [Column("last_played_time")]
    public DateTime LastPlayed { get; set; }
}