namespace Edo.Receipt.Sender
{
	public sealed class ReceiptCorrectionCirculationResult
	{
		private ReceiptCorrectionCirculationResult(
			ReceiptCorrectionCirculationDecision decision,
			string message,
			int? transferIterationId)
		{
			Decision = decision;
			Message = message;
			TransferIterationId = transferIterationId;
		}

		public ReceiptCorrectionCirculationDecision Decision { get; }

		public string Message { get; }

		public int? TransferIterationId { get; }

		public static ReceiptCorrectionCirculationResult Send()
		{
			return new ReceiptCorrectionCirculationResult(ReceiptCorrectionCirculationDecision.Send, null, null);
		}

		public static ReceiptCorrectionCirculationResult Wait(string message, int? transferIterationId = null)
		{
			return new ReceiptCorrectionCirculationResult(
				ReceiptCorrectionCirculationDecision.Wait,
				message,
				transferIterationId);
		}

		public static ReceiptCorrectionCirculationResult Reject(string message)
		{
			return new ReceiptCorrectionCirculationResult(ReceiptCorrectionCirculationDecision.Reject, message, null);
		}
	}
}
