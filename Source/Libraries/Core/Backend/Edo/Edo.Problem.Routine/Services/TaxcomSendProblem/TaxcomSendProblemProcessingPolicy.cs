using System;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.TaxcomSendProblem
{
	public static class TaxcomSendProblemProcessingPolicy
	{
		public static bool CanRetry(
			EdoTaskProblemRoutineState state,
			DateTime now,
			TimeSpan workerInterval)
		{
			if(state is null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			return !state.LastRetryTime.HasValue
				|| state.LastRetryTime.Value + workerInterval <= now;
		}

		public static bool ShouldRequestNotification(
			EdoTaskProblemRoutineState state,
			int maxAttempts)
		{
			if(state is null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			return state.RetryCount >= maxAttempts;
		}
	}
}
