using System;

namespace DateTimeHelpers
{
	public interface IDateTimeSlice
	{
		DateTime EndDate { get; set; }
		int Quarter { get; }
		DateTimeSliceType SliceType { get; set; }
		DateTime StartDate { get; set; }
		int WeekNumber { get; }

		string ToString();
	}
}