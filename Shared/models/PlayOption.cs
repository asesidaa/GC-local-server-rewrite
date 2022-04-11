using Shared.enums;

namespace Shared.models;

public class PlayOption
{
    public long CardId { get; set; }

    public PlayOptions.FastSlowIndicator FastSlowIndicator { get; set; }

    public PlayOptions.FeverTranceShow FeverTrance { get; set; }
}