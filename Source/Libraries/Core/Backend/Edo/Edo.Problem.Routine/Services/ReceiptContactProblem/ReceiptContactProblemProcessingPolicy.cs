using System;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.ReceiptContactProblem
{
	/// <summary>
	/// Правила повторной обработки проблем с контактом для отправки чека.
	/// </summary>
	public static class ReceiptContactProblemProcessingPolicy
	{
		/// <summary>
		/// Проверяет, истек ли интервал с момента предыдущей попытки.
		/// </summary>
		/// <param name="state">Состояние обработки проблемы.</param>
		/// <param name="now">Текущее время.</param>
		/// <param name="workerInterval">Минимальный интервал между попытками.</param>
		/// <returns><see langword="true"/>, если можно выполнить следующую попытку.</returns>
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

		/// <summary>
		/// Проверяет, достигнуто ли количество попыток, после которого требуется уведомление.
		/// </summary>
		/// <param name="state">Состояние обработки проблемы.</param>
		/// <param name="retryAttemptsBeforeNotification">Количество попыток до уведомления.</param>
		/// <returns><see langword="true"/>, если требуется сформировать уведомление.</returns>
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
