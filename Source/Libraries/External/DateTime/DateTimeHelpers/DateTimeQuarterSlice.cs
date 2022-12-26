namespace DateTimeHelpers
{
	public sealed class DateTimeQuarterSlice : DateTimeSlice
	{
		public override string ToString() => $"{Quarter}кв{StartDate:yy}";
	}
}
