namespace Application.Common.Helpers;

public static class TimeHelper
{
    public static string CurrentTimeToString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
    }
}