using System;

namespace VodovozInfrastructure.Utils {
    public static class DatesUtils {
        public static Tuple<DateTime, DateTime> GetStartAndEndOfTheDay(DateTime date)
            => new Tuple<DateTime, DateTime>(date.Date, date.Date.AddDays(1).AddTicks(-1));

        public static DateTime EndOfDay(this DateTime date)
            => new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        
        public static DateTime StartOfDay(this DateTime date)
            => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        
    }
}