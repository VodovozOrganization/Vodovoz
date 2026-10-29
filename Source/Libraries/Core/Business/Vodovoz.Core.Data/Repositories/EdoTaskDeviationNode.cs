using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Зарегистрированное отклонение вместе с кодами связанных с ним сущностей
	/// </summary>
	public class EdoTaskDeviationNode
	{
		/// <summary>
		/// Отклонение
		/// </summary>
		public EdoTaskDeviation Deviation { get; set; }

		/// <summary>
		/// Идентификатор задачи ЭДО, по которой зафиксировано отклонение.
		/// Пусто, если отклонение зарегистрировано по заявке, еще не ставшей задачей
		/// </summary>
		public int? EdoTaskId { get; set; }

		/// <summary>
		/// Идентификатор заявки ЭДО, по которой зафиксировано отклонение
		/// </summary>
		public int? EdoRequestId { get; set; }

		/// <summary>
		/// Идентификатор источника отклонения, которым отклонение было зарегистрировано
		/// </summary>
		public int? DeviationSourceId { get; set; }
	}
}
