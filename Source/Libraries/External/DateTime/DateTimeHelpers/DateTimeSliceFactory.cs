using System;
using System.Collections.Generic;

namespace DateTimeHelpers
{
	public class DateTimeSliceFactory
	{
		public static IEnumerable<IDateTimeSlice> CreateSlices(DateTimeSliceType sliceType, DateTime startDate, DateTime endDate)
		{
			switch(sliceType)
			{
				case DateTimeSliceType.Day:
					return CreateDaysSlices(startDate, endDate);
				case DateTimeSliceType.Week:
					return CreateWeeksSlices(startDate, endDate);
				case DateTimeSliceType.Month:
					return CreateMonthsSlices(startDate, endDate);
				case DateTimeSliceType.Quarter:
					return CreateQuartersSlices(startDate, endDate);
				case DateTimeSliceType.Year:
					return CreateYearsSlices(startDate, endDate);
				default:
					throw new InvalidOperationException("Unsupported SlicingType");
			};
		}

		public static IEnumerable<DateTimeYearSlice> CreateYearsSlices(DateTime startDate, DateTime endDate)
		{
			var slices = new List<DateTimeYearSlice>();
			var date = startDate.Date;

			while(date <= endDate)
			{
				slices.Add(endDate.Year == date.Year
					? DateTimeYearSlice.Create(date.Date, endDate.LatestDayTime())
					: DateTimeYearSlice.Create(date.Date, date.LastDayOfYear().LatestDayTime()));

				date = date.AddYears(1).FirstDayOfYear();
			}

			return slices;
		}

		public static IEnumerable<DateTimeQuarterSlice> CreateQuartersSlices(DateTime startDate, DateTime endDate)
		{
			var slices = new List<DateTimeQuarterSlice>();
			var date = startDate.Date;

			while(date <= endDate)
			{
				if(date.Year == endDate.Year
					&& date.GetQuarter() == endDate.GetQuarter())
				{
					slices.Add(DateTimeQuarterSlice.Create(date.Date, endDate.Date.LatestDayTime()));
				}
				else
				{
					slices.Add(DateTimeQuarterSlice.Create(date.Date, date.LastQuarterDay().LatestDayTime()));
				}

				date = date.LastQuarterDay().AddDays(1);
			}

			return slices;
		}

		public static IEnumerable<DateTimeMonthSlice> CreateMonthsSlices(DateTime startDate, DateTime endDate)
		{
			var slices = new List<DateTimeMonthSlice>();
			var date = startDate.Date;

			while(date <= endDate)
			{
				if(date.Month == endDate.Month
					&& date.Year == endDate.Year)
				{
					slices.Add(DateTimeMonthSlice.Create(date.Date, endDate.LatestDayTime()));
				}
				else
				{
					var sliceEnd = date.LastDayOfMonth();
					slices.Add(DateTimeMonthSlice.Create(date.Date, sliceEnd.LatestDayTime()));
				}

				date = date.AddMonths(1).FirstDayOfMonth();
			}

			return slices;
		}

		public static IEnumerable<DateTimeWeekSlice> CreateWeeksSlices(DateTime startDate, DateTime endDate)
		{
			var slices = new List<DateTimeWeekSlice>();
			var date = startDate.Date;

			while(date <= endDate)
			{
				if(date.Year == endDate.Year
					&& date.Month == endDate.Month
					&& date.Date.GetWeekNumber() == endDate.Date.GetWeekNumber())
				{
					slices.Add(DateTimeWeekSlice.Create(date.Date, endDate.LastDayOfWeek().LatestDayTime()));
				}
				else
				{
					var weekLastDay = date.LastDayOfWeek();
					var sliceEnd = weekLastDay.GetWeekNumber() == date.GetWeekNumber() ? weekLastDay : date.LastDayOfYear();

					slices.Add(DateTimeWeekSlice.Create(date.Date, sliceEnd.LatestDayTime()));
				}
				
				date = date.AddWeeks(1).FirstDayOfWeek();
			}

			return slices;
		}

		public static IEnumerable<DateTimeDaySlice> CreateDaysSlices(DateTime startDate, DateTime endDate)
		{
			var slices = new List<DateTimeDaySlice>();
			var date = startDate.Date;

			while(date <= endDate)
			{
				slices.Add(date.Date == endDate.Date
					? DateTimeDaySlice.Create(date.Date, endDate.LatestDayTime())
					: DateTimeDaySlice.Create(date.Date, date.LatestDayTime()));

				date = date.AddDays(1);
			}

			return slices;
		}
	}
}
