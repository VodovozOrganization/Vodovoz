namespace DateTimeHelpers
{
	public sealed class DateTimeWeekSlice : DateTimeSlice
	{
		public override string ToString() => $"{WeekNumber}нед{StartDate:yy}";
	}
}
