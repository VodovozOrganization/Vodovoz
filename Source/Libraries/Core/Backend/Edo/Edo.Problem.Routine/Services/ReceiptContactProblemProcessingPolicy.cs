using System;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	public static class ReceiptContactProblemProcessingPolicy
	{
		public static bool CanRetry(
			EdoTaskProblemRoutineState state,
			DateTime now,
			TimeSpan workerInterval)
		{
			if(state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			return !state.LastRetryTime.HasValue
				|| state.LastRetryTime.Value + workerInterval <= now;
		}

		public static bool ShouldRequestNotification(
			EdoTaskProblemRoutineState state,
			int retryAttemptsBeforeNotification)
		{
			if(state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			return state.RetryCount == retryAttemptsBeforeNotification;
		}
	}
}
