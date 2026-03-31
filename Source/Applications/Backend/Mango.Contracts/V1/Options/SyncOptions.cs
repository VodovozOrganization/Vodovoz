namespace Mango.Contracts.V1.Options
{
	public class SyncOptions
	{
		public const string SectionName = "Sync";

		public int PollIntervalSeconds { get; set; } = 300;
		
		public int RangeMinutes  { get; set; } = 60;
		
		public int ResultRetryCount { get; set; } = 20;
		
		public int ResultRetryDelaySeconds { get; set; } = 5;
	}
}
