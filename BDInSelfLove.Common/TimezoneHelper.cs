namespace BDInSelfLove.Common
{
    using System;

    using TimeZoneConverter;

    public static class TimezoneHelper
    {
        public static TimeZoneInfo GetUserWindowsTimezone(string timezone)
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

            return TimeZoneInfo.ConvertTimeToUtc(localTime, GetUserWindowsTimezone(timezone));
        }

        public static DateTime ToLocalTime(DateTime utcTime, string timezone)
        {
            if (timezone == null)
            {
                return utcTime;
            }

            TimeZoneInfo userWindowsTimezone = TZConvert.GetTimeZoneInfo(GetUserWindowsTimezone(timezone).Id);
            DateTime userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, userWindowsTimezone);
            return userLocalTime;
        }
    }
}
