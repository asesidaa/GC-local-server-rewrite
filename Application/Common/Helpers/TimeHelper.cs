namespace Application.Common.Helpers;

public static class TimeHelper
{
    public static string CurrentTimeToString()
    {
        return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
    }

    public static string DateToString(DateTime time)
    {
        return time.ToString("yyyy/MM/dd HH:mm:ss");
    }
}