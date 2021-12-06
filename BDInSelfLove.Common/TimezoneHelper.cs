namespace BDInSelfLove.Common
{
    using System;

    using TimeZoneConverter;

    public static class TimezoneHelper
    {
        public static TimeZoneInfo GetWindowsTimezone(string timezone)
        {
            if (timezone == null)
            {
                return null;
            }

            return TZConvert.GetTimeZoneInfo(timezone);
        }

        public static DateTime ToUTCTime(DateTime localTime, string timezone)
        {
            if (timezone == null)
            {
                return localTime;
            }

            return TimeZoneInfo.ConvertTimeToUtc(localTime, GetWindowsTimezone(timezone));
        }

        public static DateTime ToLocalTime(DateTime utcTime, string timezone)
        {
            if (timezone == null)
            {
                return utcTime;
            }

            TimeZoneInfo userWindowsTimezone = TZConvert.GetTimeZoneInfo(GetWindowsTimezone(timezone).Id);
            DateTime userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, userWindowsTimezone);
            return userLocalTime;
        }
    }
}
