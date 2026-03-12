namespace Mango.Contracts.V1.Options
{
	public class SyncOptions
	{
		public const string SectionName = "Sync";

		public int PollIntervalSeconds { get; set; } = 300;
		
		public int OverlapMinutes { get; set; } = 10;
		
		public int RangeMinutes  { get; set; } = 60;

		public int IntersectionRangeMinute { get; set; } = 5;
		
		public int ResultRetryCount { get; set; } = 20;
		
		public int ResultRetryDelaySeconds { get; set; } = 5;
	}
}
