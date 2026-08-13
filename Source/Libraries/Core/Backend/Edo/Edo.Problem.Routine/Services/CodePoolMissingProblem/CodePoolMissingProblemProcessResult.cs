namespace Edo.Problem.Routine.Services.CodePoolMissingProblem
{
	public readonly struct CodePoolMissingProblemProcessResult
	{
		public static CodePoolMissingProblemProcessResult Empty => new CodePoolMissingProblemProcessResult(false, false);

		public CodePoolMissingProblemProcessResult(bool processed, bool notificationSent)
		{
			Processed = processed;
			NotificationSent = notificationSent;
		}

		public bool Processed { get; }
		public bool NotificationSent { get; }
	}
}
