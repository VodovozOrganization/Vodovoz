using System;

namespace Mindbox.Library.Services
{
	public sealed class MindboxLoyaltyOrderResult
	{
		public MindboxLoyaltyOrderOutcome Outcome { get; private set; }
		public bool IsSuccess => Outcome == MindboxLoyaltyOrderOutcome.Success;
		public long? MindboxOrderId { get; private set; }
		public int BonusesWrittenOff { get; private set; }
		public int? BonusesToEarn { get; private set; }
		public int HttpStatusCode { get; private set; }
		public string Status { get; private set; }
		public string ProcessingStatus { get; private set; }
		public string ErrorId { get; private set; }
		public string ErrorMessage { get; private set; }
		public long DurationMs { get; private set; }

		public static MindboxLoyaltyOrderResult Success(
			long? mindboxOrderId,
			int bonusesWrittenOff,
			int? bonusesToEarn,
			int httpStatusCode,
			string status,
			string processingStatus,
			string errorId,
			long durationMs)
		{
			return new MindboxLoyaltyOrderResult
			{
				Outcome = MindboxLoyaltyOrderOutcome.Success,
				MindboxOrderId = mindboxOrderId,
				BonusesWrittenOff = bonusesWrittenOff,
				BonusesToEarn = bonusesToEarn,
				HttpStatusCode = httpStatusCode,
				Status = status,
				ProcessingStatus = processingStatus,
				ErrorId = errorId,
				DurationMs = durationMs
			};
		}

		public static MindboxLoyaltyOrderResult Failure(
			MindboxLoyaltyOrderOutcome outcome,
			string errorMessage,
			int httpStatusCode = 0,
			string status = null,
			string processingStatus = null,
			string errorId = null,
			long durationMs = 0)
		{
			return new MindboxLoyaltyOrderResult
			{
				Outcome = outcome,
				ErrorMessage = errorMessage,
				HttpStatusCode = httpStatusCode,
				Status = status,
				ProcessingStatus = processingStatus,
				ErrorId = errorId,
				DurationMs = durationMs
			};
		}
	}
}
