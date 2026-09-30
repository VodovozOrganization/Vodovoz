namespace Edo.Problem.Routine.Services.TaxcomSendProblem
{
	public class TaxcomSendProblemNotificationData
	{
		public int OrderId { get; }
		public string MainDocumentId { get; }
		public string ErrorMessage { get; }
		public int ProblemId { get; }
		public int RetryCount { get; }

		public TaxcomSendProblemNotificationData(
			int orderId,
			string mainDocumentId,
			string errorMessage,
			int problemId,
			int retryCount)
		{
			OrderId = orderId;
			MainDocumentId = mainDocumentId;
			ErrorMessage = errorMessage;
			ProblemId = problemId;
			RetryCount = retryCount;
		}
	}
}
