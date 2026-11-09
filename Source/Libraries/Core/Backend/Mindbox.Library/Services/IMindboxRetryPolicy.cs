using System;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Расписание повторных попыток отправки в Mindbox
	/// </summary>
	public interface IMindboxRetryPolicy
	{
		/// <summary>
		/// Возвращает дату следующей попытки или <c>null</c>, если повторы исчерпаны
		/// </summary>
		/// <param name="now">Текущее время</param>
		/// <param name="attempts">Количество уже выполненных попыток</param>
		/// <param name="retryUntil">Время, после которого повторы прекращаются</param>
		/// <returns>Дата следующей попытки или <c>null</c>, если повтор не требуется</returns>
		DateTime? GetNextAttemptAt(DateTime now, int attempts, DateTime retryUntil);
	}
}
