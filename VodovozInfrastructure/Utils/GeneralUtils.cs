using System;
using System.Collections.Generic;
using System.Linq;

namespace VodovozInfrastructure.Utils
{
    public static class GeneralUtils
    {
        //new DateTime(2020, 1, 1).Range(new DateTime(2020, 1, 31)); даст перечисление дней между датами
        public static IEnumerable<DateTime> Range(this DateTime startDate, DateTime endDate)
        {
            return Enumerable.Range(0, (endDate - startDate).Days + 1).Select(d => startDate.AddDays(d));
        }
    }
}