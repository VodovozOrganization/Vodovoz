using System;

namespace Edo.Problem.Routine.Options
{
	public class TaxcomSendProblemWorkerOptions
	{
		/// <summary>
		/// Интервал проверки задач
		/// </summary>
		public TimeSpan WorkerInterval { get; set; }

		/// <summary>
		/// Размер пакета задач для обработки
		/// </summary>
		public int BatchSize { get; set; }

		/// <summary>
		/// Максимальное количество попыток (всего 3)
		/// </summary>
		public int MaxAttempts { get; set; }

		/// <summary>
		/// Задержки между попытками: 1 час, 1 день, 3 дня
		/// </summary>
		public TimeSpan[] RetryDelays { get; set; } = new[]
		{
			TimeSpan.FromHours(1),
			TimeSpan.FromDays(1),
			TimeSpan.FromDays(3)
		};

		/// <summary>
		/// Минимальное время создания задачи для обработки
		/// </summary>
		public TimeSpan MinTaskAge { get; set; }
	}
}
