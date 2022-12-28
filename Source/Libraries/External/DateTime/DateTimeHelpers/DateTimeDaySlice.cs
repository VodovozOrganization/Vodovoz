namespace DateTimeHelpers
{
	public sealed class DateTimeDaySlice : DateTimeSlice
	{
		public override string ToString() => StartDate.ToString("dd.MM.yy");
	}
}
