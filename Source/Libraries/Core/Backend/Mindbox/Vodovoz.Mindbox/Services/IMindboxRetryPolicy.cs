using System;

namespace Vodovoz.Mindbox.Services
{
	/// <summary>
	/// Расписание повторных попыток отправки в Mindbox
	/// </summary>
	public interface IMindboxRetryPolicy
	{
		/// <summary>
		/// Возвращает дату следующей попытки или null, если повторы исчерпаны
		/// </summary>
		DateTime? GetNextAttemptAt(DateTime now, int attempts, DateTime retryUntil);
	}
}
