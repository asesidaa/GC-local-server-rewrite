namespace SharedProject.enums;

public static class Extensions
{
    public static string GetHelpText(this PlayOptions.FastSlowIndicator indicator)
    {
        return indicator switch
        {
            PlayOptions.FastSlowIndicator.NotUsed => "Do not show FAST/SLOW indicator",
            PlayOptions.FastSlowIndicator.NearAvatar => "Show FAST/SLOW indicator near avatar",
            PlayOptions.FastSlowIndicator.NearJudgement => "Show FAST/SLOW indicator near judgement text",
            _ => throw new ArgumentOutOfRangeException(nameof(indicator), indicator, null)
        };
    }

    public static string GetHelpText(this PlayOptions.FeverTranceShow show)
    {
        return show switch
        {
            PlayOptions.FeverTranceShow.NotUsed => "Do not show FEVER/TRANCE",
            PlayOptions.FeverTranceShow.Show => "Show FEVER/TRANCE",
            _ => throw new ArgumentOutOfRangeException(nameof(show), show, null)
        };
    }
}