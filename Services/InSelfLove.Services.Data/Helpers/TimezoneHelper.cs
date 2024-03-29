﻿namespace InSelfLove.Services.Data.Helpers
{
    using System;

    using TimeZoneConverter;

    public static class TimezoneHelper
    {
        public static TimeZoneInfo GetTimezoneInfo(string timezone)
        {
            try
            {
                return TZConvert.GetTimeZoneInfo(timezone);
            }
            catch (Exception)
            {
                // Set timezone to default value if non-existent timezone is provided
                return TZConvert.GetTimeZoneInfo("Europe/Sofia");
            }
        }

        public static DateTime ToUTCTime(DateTime localTime, string timezone)
        {
            if (timezone == null)
            {
                return localTime;
            }

            // Datetime is UTC by default which doesn't fit our purpose -> set to unspecified and convert to utc using the client local timezone
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified), GetTimezoneInfo(timezone));
        }

        public static DateTime ToLocalTime(DateTime utcTime, string timezone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, GetTimezoneInfo(timezone));
        }
    }
}
