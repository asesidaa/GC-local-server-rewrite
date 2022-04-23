using SharedProject.enums;

namespace SharedProject.models;

public class PlayOption
{
    public long CardId { get; set; }

    public PlayOptions.FastSlowIndicator FastSlowIndicator { get; set; }

    public PlayOptions.FeverTranceShow FeverTrance { get; set; }
}