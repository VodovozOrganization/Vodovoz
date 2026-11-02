using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Состояние документооборота у провайдера ЭДО
	/// </summary>
	public interface IEdoDocflowMonitoringNode
	{
		/// <summary>
		/// Идентификатор задачи ЭДО
		/// </summary>
		int EdoTaskId { get; }

		/// <summary>
		/// Время создания исходящего документа
		/// </summary>
		DateTime? OutgoingDocumentCreationTime { get; set; }

		/// <summary>
		/// Статус исходящего документа
		/// </summary>
		EdoDocumentStatus? OutgoingDocumentStatus { get; set; }

		/// <summary>
		/// Время создания документооборота у провайдера ЭДО
		/// </summary>
		DateTime? DocflowCreationTime { get; set; }

		/// <summary>
		/// Время изменения текущего состояния документооборота у провайдера ЭДО
		/// </summary>
		DateTime? LastActionTime { get; set; }

		/// <summary>
		/// Текущее состояние документооборота у провайдера ЭДО
		/// </summary>
		EdoDocFlowStatus? LastActionState { get; set; }

		/// <summary>
		/// Признак текущего состояния, отличного от начального <see cref="EdoDocFlowStatus.NotStarted"/>
		/// </summary>
		bool HasProviderAnswer { get; set; }

		/// <summary>
		/// Время текущего состояния получения документа оператором
		/// </summary>
		DateTime? FirstSentActionTime { get; set; }

		/// <summary>
		/// Время текущего состояния успешного завершения документооборота
		/// </summary>
		DateTime? SucceedActionTime { get; set; }

		/// <summary>
		/// Время текущего состояния ожидания аннулирования
		/// </summary>
		DateTime? WaitingCancellationActionTime { get; set; }

		/// <summary>
		/// Время текущего состояния аннулирования документооборота
		/// </summary>
		DateTime? CancelledActionTime { get; set; }

		/// <summary>
		/// Статус прослеживаемости в ГИС МТ текущего состояния
		/// </summary>
		TrueMarkTraceabilityStatus? TraceabilityStatus { get; set; }

		/// <summary>
		/// Время текущего состояния с результатом прослеживаемости в ГИС МТ
		/// </summary>
		DateTime? TraceabilityActionTime { get; set; }
	}
}
