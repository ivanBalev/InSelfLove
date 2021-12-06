using System;
using System.Globalization;

namespace BDInSelfLove.Common
{
    public static class GlobalValues
    {
        public const string SystemName = "InSelfLove";

        public const string AdministratorRoleName = "Administrator";

        public const string UserRoleName = "User";

        public static DateTime WorkDayStartUTC = DateTime.ParseExact("07:00:00", "HH:mm:ss", CultureInfo.InvariantCulture);

        public static DateTime WorkDayEndUTC = DateTime.ParseExact("15:00:00", "HH:mm:ss", CultureInfo.InvariantCulture);
    }
}
