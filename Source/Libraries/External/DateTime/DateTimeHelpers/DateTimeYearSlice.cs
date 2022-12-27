namespace DateTimeHelpers
{
	public sealed class DateTimeYearSlice : DateTimeSlice
	{
		public override string ToString() => StartDate.ToString("yyyy");
	}
}
