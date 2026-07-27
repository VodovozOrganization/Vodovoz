using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Infrastructure.Scheduling
{
	/// <summary>
	/// Планировщик ежедневного запуска воркеров в заданное время
	/// </summary>
	public interface IDailyScheduler
	{
		/// <summary>
		/// Ожидает до следующего запланированного времени запуска.
		/// Если время уже прошло сегодня — планирует на завтра.
		/// </summary>
		/// <param name="timeOfDay">Время суток (например: new TimeSpan(8, 30, 0))</param>
		/// <param name="workerName">Название воркера для логирования</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task DelayUntilNextOccurrenceAsync(
			TimeSpan timeOfDay,
			string workerName,
			CancellationToken cancellationToken = default);
	}
}
