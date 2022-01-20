namespace BDInSelfLove.Common
{
    using System;

    using TimeZoneConverter;

    public static class TimezoneHelper
    {
        public static TimeZoneInfo GetTimezone(string timezoneId)
        {
            if (timezoneId == null)
            {
                return null;
            }

            return TZConvert.GetTimeZoneInfo(timezoneId);
        }

        public static DateTime ToUTCTime(DateTime localTime, string timezone)
        {
            if (timezone == null)
            {
                return localTime;
            }

            return TimeZoneInfo.ConvertTimeToUtc(localTime, GetTimezone(timezone));
        }

        public static DateTime ToLocalTime(DateTime utcTime, string timezone)
        {
            if (timezone == null)
            {
                return utcTime;
            }

            TimeZoneInfo userWindowsTimezone = TZConvert.GetTimeZoneInfo(GetTimezone(timezone).Id);
            DateTime userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, userWindowsTimezone);
            return userLocalTime;
        }
    }
}
