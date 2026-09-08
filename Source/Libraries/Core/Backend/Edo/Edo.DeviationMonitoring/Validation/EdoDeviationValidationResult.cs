using System;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Обнаруженное отклонение документооборота ЭДО
	/// </summary>
	public class EdoDeviationValidationResult
	{
		/// <summary>
		/// Код описания отклонения в справочнике
		/// </summary>
		public int DeviationSourceId { get; set; }

		/// <summary>
		/// Код задачи ЭДО. Пустой, если задача по заявке не была создана
		/// </summary>
		public int? EdoTaskId { get; set; }

		/// <summary>
		/// Код заявки ЭДО
		/// </summary>
		public int? EdoRequestId { get; set; }

		/// <summary>
		/// Название стадии, на которой обнаружено отклонение
		/// </summary>
		public string StageName { get; set; }

		/// <summary>
		/// Время, от которого отсчитывался таймаут
		/// </summary>
		public DateTime StageStartTime { get; set; }

		/// <summary>
		/// Превышенный таймаут
		/// </summary>
		public TimeSpan Threshold { get; set; }

		/// <summary>
		/// Детализация по отклонению
		/// </summary>
		public string Details { get; set; }
	}
}
