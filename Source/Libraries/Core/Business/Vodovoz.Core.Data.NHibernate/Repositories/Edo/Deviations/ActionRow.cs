using System;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка действия документооборота у провайдера ЭДО
	/// </summary>
	internal class ActionRow
	{
		/// <summary>
		/// Идентификатор действия
		/// </summary>
		public int ActionId { get; set; }

		/// <summary>
		/// Идентификатор документооборота, к которому относится действие
		/// </summary>
		public int DocflowId { get; set; }

		/// <summary>
		/// Время действия
		/// </summary>
		public DateTime Time { get; set; }

		/// <summary>
		/// Состояние документооборота, зафиксированное действием
		/// </summary>
		public EdoDocFlowStatus State { get; set; }

		/// <summary>
		/// Результат обработки кодов маркировки в ГИС МТ
		/// Пустой, если действие о прослеживаемости ничего не сообщает
		/// </summary>
		public TrueMarkTraceabilityStatus? TraceabilityStatus { get; set; }
	}
}
