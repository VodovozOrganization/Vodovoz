using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Зарегистрированное отклонение вместе с кодами связанных с ним сущностей.
	/// <para>
	/// Коды задачи, заявки и источника отклонения читаются тем же запросом, что и само отклонение:
	/// обращение к ссылочным свойствам подняло бы задачу и заявку из базы по одному запросу
	/// на каждое отклонение, а сервису снятия нужны только их коды
	/// </para>
	/// </summary>
	public class EdoTaskDeviationNode
	{
		/// <summary>
		/// Отклонение
		/// </summary>
		public EdoTaskDeviation Deviation { get; set; }

		/// <summary>
		/// Код задачи ЭДО, по которой зафиксировано отклонение.
		/// Пусто, если отклонение зарегистрировано по заявке, еще не ставшей задачей
		/// </summary>
		public int? EdoTaskId { get; set; }

		/// <summary>
		/// Код заявки ЭДО, по которой зафиксировано отклонение
		/// </summary>
		public int? EdoRequestId { get; set; }

		/// <summary>
		/// Код источника отклонения, которым отклонение было зарегистрировано
		/// </summary>
		public int? DeviationSourceId { get; set; }
	}
}
