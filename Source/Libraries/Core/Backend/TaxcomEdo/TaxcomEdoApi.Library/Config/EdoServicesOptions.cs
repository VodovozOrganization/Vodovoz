namespace TaxcomEdoApi.Library.Config
{
	public sealed class EdoServicesOptions
	{
		public const string Path = "ServicesOptions";
		
		public int DelayBetweenAutoSendReceiveProcessingInSeconds { get; set; }
	}
}
