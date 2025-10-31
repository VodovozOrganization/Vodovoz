using System;
using System.Collections.Generic;

namespace Core.Infrastructure
{
	public static class DateGenerator
	{
		public static IEnumerable<DateTime> GenerateDates(DateTime startDate, DateTime endDate)
		{
			var dateTimes = new List<DateTime>();
			var daysCount = (endDate.Date - startDate.Date).Days + 1;

			for(var i = 0; i < daysCount; i++)
			{
				if(i == 0)
				{
					dateTimes.Add(startDate);
					continue;
				}
				
				dateTimes.Add(startDate.AddDays(i));
			}
			
			return dateTimes;
		}
	}
}
