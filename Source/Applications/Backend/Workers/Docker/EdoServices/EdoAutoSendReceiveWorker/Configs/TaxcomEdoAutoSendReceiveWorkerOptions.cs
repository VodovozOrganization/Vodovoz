namespace EdoAutoSendReceiveWorker.Configs
{
	public class TaxcomEdoAutoSendReceiveWorkerOptions
	{
		public const string Path = nameof(TaxcomEdoAutoSendReceiveWorkerOptions);
		
		public int DelayBetweenAutoSendReceiveProcessingInSeconds { get; set; }
	}
}
