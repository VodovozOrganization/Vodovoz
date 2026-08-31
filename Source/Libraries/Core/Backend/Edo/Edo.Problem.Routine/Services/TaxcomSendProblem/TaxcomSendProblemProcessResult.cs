namespace Edo.Problem.Routine.Services.TaxcomSendProblem
{
	public readonly struct TaxcomSendProblemProcessResult
	{
		public static TaxcomSendProblemProcessResult Empty => new TaxcomSendProblemProcessResult(false, false, null);

		public bool Processed { get; }
		public bool ShouldNotify { get; }
		public TaxcomSendProblemNotificationData NotificationData { get; }

		public TaxcomSendProblemProcessResult(
			bool processed,
			bool shouldNotify,
			TaxcomSendProblemNotificationData notificationData)
		{
			Processed = processed;
			ShouldNotify = shouldNotify;
			NotificationData = notificationData;
		} 
	}
}
