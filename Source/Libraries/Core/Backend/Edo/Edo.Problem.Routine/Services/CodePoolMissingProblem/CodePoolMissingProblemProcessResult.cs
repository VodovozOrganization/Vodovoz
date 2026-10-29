namespace Edo.Problem.Routine.Services.CodePoolMissingProblem
{
	public readonly struct CodePoolMissingProblemProcessResult
	{
		public static CodePoolMissingProblemProcessResult Empty => new CodePoolMissingProblemProcessResult(false, false, null);

		public bool Processed { get; }
		public bool ShouldNotify { get; }
		public CodePoolMissingProblemNotificationData NotificationData { get; }

		public CodePoolMissingProblemProcessResult(
			bool processed,
			bool shouldNotify,
			CodePoolMissingProblemNotificationData notificationData)
		{
			Processed = processed;
			ShouldNotify = shouldNotify;
			NotificationData = notificationData;
		}
	}
}
