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

			if(endDate.Year == startDate.Year)
			{
				slices.Add(new DateTimeYearSlice
				{
					StartDate = startDate.Date,
					EndDate = endDate.LatestDayTime(),
					SliceType = DateTimeSliceType.Year
				});
				return slices;
			}

			var date = startDate.Date;

			while(date <= endDate)
			{
				slices.Add(new DateTimeYearSlice
				{
					StartDate = date.Date,
					EndDate = date.LastDayOfYear().LatestDayTime(),
					SliceType = DateTimeSliceType.Year
				});

				date = date.AddYears(1).FirstDayOfYear();
			}

			return slices;
		}

		public static IEnumerable<DateTimeQuarterSlice> CreateQuartersSlices(DateTime startDate, DateTime endDate)
		{
			var slices = new List<DateTimeQuarterSlice>();

			if(startDate.Year == endDate.Year
			&& startDate.GetQuarter() == endDate.GetQuarter())
			{
				slices.Add(new DateTimeQuarterSlice
				{
					StartDate = startDate.Date,
					EndDate = endDate.Date.LatestDayTime(),
					SliceType = DateTimeSliceType.Quarter
				});

				return slices;
			}

			var date = startDate.Date;

			while(date <= endDate)
			{
				slices.Add(new DateTimeQuarterSlice
				{
					StartDate = date.Date,
					EndDate = date.LastQuarterDay().LatestDayTime(),
					SliceType = DateTimeSliceType.Quarter
				});

				date = date.LastQuarterDay().AddDays(1);
			}

			return slices;
		}

		public static IEnumerable<DateTimeMonthSlice> CreateMonthsSlices(DateTime startDate, DateTime endDate)
		{
			var slices = new List<DateTimeMonthSlice>();

			if(startDate.Year == endDate.Year
			&& startDate.Month == endDate.Month)
			{
				slices.Add(new DateTimeMonthSlice
				{
					StartDate = startDate.Date,
					EndDate = endDate.LatestDayTime(),
					SliceType = DateTimeSliceType.Month
				});

				return slices;
			}

			var date = startDate.Date;

			while(date <= endDate)
			{
				var sliceEnd = date.AddMonths(1).AddDays(-1);

				slices.Add(new DateTimeMonthSlice
				{
					StartDate = date.Date,
					EndDate = sliceEnd.LatestDayTime(),
					SliceType = DateTimeSliceType.Month
				});

				date = date.AddMonths(1).FirstDayOfMonth();
			}

			return slices;
		}

		public static IEnumerable<DateTimeWeekSlice> CreateWeeksSlices(DateTime startDate, DateTime endDate)
		{
			var slices = new List<DateTimeWeekSlice>();

			if(startDate.Year == endDate.Year
			&& startDate.Date.GetWeekNumber() == endDate.Date.GetWeekNumber())
			{
				slices.Add(new DateTimeWeekSlice
				{
					StartDate = startDate.Date.FirstDayOfWeek(),
					EndDate = endDate.LastDayOfWeek().LatestDayTime(),
					SliceType = DateTimeSliceType.Week
				});
				return slices;
			}

			var date = startDate.Date.FirstDayOfWeek();

			while(date <= endDate)
			{
				var weekLastDay = date.LastDayOfWeek();

				var sliceEnd = weekLastDay.GetWeekNumber() == date.GetWeekNumber() ? weekLastDay : date.LastDayOfYear();

				slices.Add(new DateTimeWeekSlice
				{
					StartDate = date.Date,
					EndDate = sliceEnd.LatestDayTime(),
					SliceType = DateTimeSliceType.Week
				});
				date = date.AddWeeks(1).FirstDayOfWeek();
			}

			return slices;
		}

		public static IEnumerable<DateTimeDaySlice> CreateDaysSlices(DateTime startDate, DateTime endDate)
		{
			var slices = new List<DateTimeDaySlice>();

			if(startDate.Date == endDate.Date)
			{
				slices.Add(new DateTimeDaySlice
				{
					StartDate = startDate.Date,
					EndDate = endDate.LatestDayTime(),
					SliceType = DateTimeSliceType.Day
				});
				return slices;
			}

			var date = startDate.Date;

			while(date <= endDate)
			{
				slices.Add(new DateTimeDaySlice
				{
					StartDate = date.Date,
					EndDate = date.LatestDayTime(),
					SliceType = DateTimeSliceType.Day
				});
				date = date.AddDays(1);
			}

			return slices;
		}
	}
}
