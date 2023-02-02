﻿namespace InSelfLove.Services.Data.Helpers
{
    using System;

    using TimeZoneConverter;

    public static class TimezoneHelper
    {
        public static TimeZoneInfo GetTimezone(string timezoneId)
        {
            try
            {
                return TZConvert.GetTimeZoneInfo(timezoneId);
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

            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified), GetTimezone(timezone));
        }

        public static DateTime ToLocalTime(DateTime utcTime, string timezone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, GetTimezone(timezone));
        }
    }
}