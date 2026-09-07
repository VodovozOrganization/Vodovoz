using System;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка действия документооборота у провайдера ЭДО.
	/// Какое из действий считать последним, решает
	/// <see cref="EdoTaskMonitoringNodeBuilder"/>: сравнивать времена нельзя,
	/// они приходят из разных источников
	/// </summary>
	internal class ActionRow
	{
		/// <summary>
		/// Код действия. По нему определяется порядок действий документооборота
		/// </summary>
		public int ActionId { get; set; }

		/// <summary>
		/// Код документооборота, к которому относится действие
		/// </summary>
		public int DocflowId { get; set; }

		/// <summary>
		/// Время действия. У действий провайдера это метка Такскома,
		/// у заведенного нами действия <see cref="EdoDocFlowStatus.NotStarted"/> — наше время
		/// </summary>
		public DateTime Time { get; set; }

		/// <summary>
		/// Состояние документооборота, зафиксированное действием
		/// </summary>
		public EdoDocFlowStatus State { get; set; }

		/// <summary>
		/// Результат обработки кодов маркировки в ГИС МТ.
		/// Пустой, если действие о прослеживаемости ничего не сообщает
		/// </summary>
		public TrueMarkTraceabilityStatus? TraceabilityStatus { get; set; }
	}
}
