using System;

namespace DateTimeHelpers
{
	public sealed class DateTimeDaySlice : DateTimeSlice
	{
		protected DateTimeDaySlice(DateTime startDate, DateTime endDate) : base(startDate, endDate)
		{
			SliceType = DateTimeSliceType.Day;
		}
		
		public override string ToString() => StartDate.ToString("dd.MM.yy");
		
		public static DateTimeDaySlice Create(DateTime startDate, DateTime endDate)
		{
			return new DateTimeDaySlice(startDate, endDate);
		}
	}
}
