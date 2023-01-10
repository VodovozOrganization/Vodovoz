using System;

namespace DateTimeHelpers
{
	public abstract class DateTimeSlice : IDateTimeSlice
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int WeekNumber => StartDate.GetWeekNumber();
		public int Quarter => StartDate.GetQuarter();
		public DateTimeSliceType SliceType { get; set; }
		public abstract override string ToString();
	}
}
