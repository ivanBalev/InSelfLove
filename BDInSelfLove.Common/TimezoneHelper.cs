namespace BDInSelfLove.Common
{
    using System;

    using TimeZoneConverter;

    public static class TimezoneHelper
    {
        public static TimeZoneInfo GetUserWindowsTimezone(string timezone)
        {
            return timezone == null ? null : TZConvert.GetTimeZoneInfo(timezone);
        }

        public static DateTime ToUTCTime(DateTime localTime, string timezone)
        {
            return TimeZoneInfo.ConvertTimeToUtc(localTime, GetUserWindowsTimezone(timezone));
        }

        public static DateTime ToLocalTime(DateTime utcTime, string timezone)
        {
            TimeZoneInfo userWindowsTimezone = TZConvert.GetTimeZoneInfo(GetUserWindowsTimezone(timezone).Id);
            DateTime userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, userWindowsTimezone);
            return userLocalTime;
        }
    }
}
