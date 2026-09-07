using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Состояние документооборота у провайдера ЭДО, общее для задач отправки документов
	/// и задач трансфера: УПД трансфера уходит тем же трактом, что и УПД заказа,
	/// поэтому и условия отклонений по документообороту у них одинаковые
	/// </summary>
	public interface IEdoDocflowMonitoringNode
	{
		/// <summary>
		/// Код задачи ЭДО
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
		/// Время последнего действия документооборота у провайдера ЭДО
		/// </summary>
		DateTime? LastActionTime { get; set; }

		/// <summary>
		/// Состояние последнего действия документооборота у провайдера ЭДО
		/// </summary>
		EdoDocFlowStatus? LastActionState { get; set; }

		/// <summary>
		/// Признак того, что по документообороту есть хотя бы одно действие,
		/// кроме заведенного нами <see cref="EdoDocFlowStatus.NotStarted"/>,
		/// то есть провайдер ЭДО ответил
		/// </summary>
		bool HasProviderAnswer { get; set; }

		/// <summary>
		/// Время первого действия, означающего получение документа оператором
		/// </summary>
		DateTime? FirstSentActionTime { get; set; }

		/// <summary>
		/// Время завершения документооборота
		/// </summary>
		DateTime? SucceedActionTime { get; set; }

		/// <summary>
		/// Время перевода документооборота в ожидание аннулирования
		/// </summary>
		DateTime? WaitingCancellationActionTime { get; set; }

		/// <summary>
		/// Время аннулирования документооборота.
		/// Аннулирование конечно: по такому документообороту
		/// не придет ни результат ГИС МТ, ни смена статуса
		/// </summary>
		DateTime? CancelledActionTime { get; set; }

		/// <summary>
		/// Последний полученный статус прослеживаемости в ГИС МТ
		/// </summary>
		TrueMarkTraceabilityStatus? TraceabilityStatus { get; set; }

		/// <summary>
		/// Время получения последнего статуса прослеживаемости в ГИС МТ
		/// </summary>
		DateTime? TraceabilityActionTime { get; set; }
	}
}
