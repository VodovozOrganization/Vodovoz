namespace DateTimeHelpers
{
	public sealed class DateTimeMonthSlice : DateTimeSlice
	{
		public override string ToString() => StartDate.ToString("MMM.yy");
	}
}
