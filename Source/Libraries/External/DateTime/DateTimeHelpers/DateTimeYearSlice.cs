using System;

namespace DateTimeHelpers
{
	public sealed class DateTimeYearSlice : DateTimeSlice
	{
		protected DateTimeYearSlice(DateTime startDate, DateTime endDate) : base(startDate, endDate)
		{
			SliceType = DateTimeSliceType.Year;
		}
		
		public override string ToString() => StartDate.ToString("yyyy");

		public static DateTimeYearSlice Create(DateTime startDate, DateTime endDate)
		{
			return new DateTimeYearSlice(startDate, endDate);
		}
	}
}
