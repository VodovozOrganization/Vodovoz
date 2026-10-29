using System;

namespace DateTimeHelpers
{
	public sealed class DateTimeQuarterSlice : DateTimeSlice
	{
		protected DateTimeQuarterSlice(DateTime startDate, DateTime endDate) : base(startDate, endDate)
		{
			SliceType = DateTimeSliceType.Quarter;
		}
		
		public override string ToString() => $"{Quarter}кв{StartDate:yy}";
		
		public static DateTimeQuarterSlice Create(DateTime startDate, DateTime endDate)
		{
			return new DateTimeQuarterSlice(startDate, endDate);
		}
	}
}
