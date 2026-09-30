using Core.Infrastructure;
using System;
using System.Globalization;
using Vodovoz.Core.Data.Repositories;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Форматирование текстов для описаний отклонений документооборота ЭДО
	/// </summary>
	public static class EdoDeviationTextFormatter
	{
		/// <summary>
		/// Приводит длительность к виду, читаемому в журнале
		/// </summary>
		/// <param name="duration">Длительность</param>
		/// <returns>Например, "2 сут 03 ч 15 мин" или "00 ч 42 мин"</returns>
		public static string FormatDuration(TimeSpan duration)
		{
			if(duration < TimeSpan.Zero)
			{
				duration = TimeSpan.Zero;
			}

			var time = $"{duration.Hours:D2} ч {duration.Minutes:D2} мин";

			return duration.Days > 0
				? $"{duration.Days} сут {time}"
				: time;
		}

		/// <summary>
		/// Собирает общий для всех отклонений хвост описания:
		/// сколько времени стадия заняла на самом деле против допустимого
		/// </summary>
		/// <param name="elapsed">Фактическая длительность стадии</param>
		/// <param name="timeout">Превышенный таймаут</param>
		/// <returns>Например, "2 сут 03 ч 15 мин при допустимых 04 ч 00 мин"</returns>
		public static string FormatElapsed(TimeSpan elapsed, TimeSpan timeout) =>
			$"{FormatDuration(elapsed)} при допустимых {FormatDuration(timeout)}";

		/// <summary>
		/// Приводит момент времени к виду, читаемому в журнале
		/// </summary>
		/// <param name="time">Момент времени</param>
		public static string FormatTime(DateTime time) =>
			time.ToString("dd.MM.yyyy HH:mm", CultureInfo.CurrentCulture);

		/// <summary>
		/// Возвращает отображаемое название стадии, на которой находится задача ЭДО
		/// </summary>
		/// <param name="task">Состояние задачи ЭДО</param>
		public static string GetStageName(EdoTaskMonitoringNode task)
		{
			if(task is null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			if(task.DocumentStage != null)
			{
				return task.DocumentStage.Value.GetEnumDisplayName();
			}

			if(task.ReceiptStatus != null)
			{
				return task.ReceiptStatus.Value.GetEnumDisplayName();
			}

			return task.TaskStatus.GetEnumDisplayName();
		}

		/// <summary>
		/// Возвращает отображаемое название стадии, на которой находится задача трансфера
		/// </summary>
		/// <param name="transferTask">Состояние задачи трансфера</param>
		public static string GetTransferStageName(EdoTransferTaskMonitoringNode transferTask)
		{
			if(transferTask is null)
			{
				throw new ArgumentNullException(nameof(transferTask));
			}

			return $"Трансфер: {transferTask.TransferStage.GetEnumDisplayName()}";
		}
	}
}
