using System;

using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка документооборота, заведенного у провайдера ЭДО по исходящему документу
	/// </summary>
	internal class DocflowRow
	{
		/// <summary>
		/// Идентификатор документооборота
		/// </summary>
		public int DocflowId { get; set; }

		/// <summary>
		/// Идентификатор исходящего документа, по которому заведен документооборот
		/// </summary>
		public int DocumentId { get; set; }

		/// <summary>
		/// Время создания документооборота
		/// </summary>
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// Текущее состояние документооборота
		/// </summary>
		public EdoDocFlowStatus DocFlowState { get; set; }

		/// <summary>
		/// Время изменения текущего состояния
		/// </summary>
		public DateTime? StateChangeTime { get; set; }

		/// <summary>
		/// Статус прослеживаемости текущего состояния
		/// </summary>
		public TrueMarkTraceabilityStatus? TrueMarkTraceabilityStatus { get; set; }
	}
}
