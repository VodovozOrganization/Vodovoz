using System;

namespace DateTimeHelpers
{
	public sealed class DateTimeMonthSlice : DateTimeSlice
	{
		protected DateTimeMonthSlice(DateTime startDate, DateTime endDate) : base(startDate, endDate)
		{
			SliceType = DateTimeSliceType.Month;
		}
		
		public override string ToString() => StartDate.ToString("MMM.yy");
		
		public static DateTimeMonthSlice Create(DateTime startDate, DateTime endDate)
		{
			return new DateTimeMonthSlice(startDate, endDate);
		}
	}
}
