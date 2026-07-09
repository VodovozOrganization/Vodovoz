using System;

namespace DateTimeHelpers
{
	public sealed class DateTimeWeekSlice : DateTimeSlice
	{
		protected DateTimeWeekSlice(DateTime startDate, DateTime endDate) : base(startDate, endDate)
		{
			SliceType = DateTimeSliceType.Week;
		}
		
		public override string ToString() => $"{WeekNumber}нед{StartDate:yy}";
		
		public static DateTimeWeekSlice Create(DateTime startDate, DateTime endDate)
		{
			return new DateTimeWeekSlice(startDate, endDate);
		}
	}
}
