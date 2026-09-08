using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Состояние задачи ЭДО, достаточное для проверки условий отклонений
	/// сервисом мониторинга документооборота
	/// </summary>
	public class EdoTaskMonitoringNode : IEdoDocflowMonitoringNode
	{
		/// <summary>
		/// Код задачи ЭДО
		/// </summary>
		public int EdoTaskId { get; set; }

		/// <summary>
		/// Тип задачи ЭДО
		/// </summary>
		public EdoTaskType TaskType { get; set; }

		/// <summary>
		/// Статус задачи ЭДО
		/// </summary>
		public EdoTaskStatus TaskStatus { get; set; }

		/// <summary>
		/// Время создания задачи ЭДО
		/// </summary>
		public DateTime TaskCreationTime { get; set; }

		/// <summary>
		/// Время начала обработки задачи ЭДО.
		/// У задачи отправки чека проставляется при переходе к отправке в кассу
		/// </summary>
		public DateTime? TaskStartTime { get; set; }

		/// <summary>
		/// Признак того, что задача завершена или отменена
		/// </summary>
		public bool IsFinished { get; set; }

		/// <summary>
		/// Признак наличия активной зарегистрированной проблемы по задаче
		/// </summary>
		public bool HasActiveProblem { get; set; }

		/// <summary>
		/// Код заявки ЭДО
		/// </summary>
		public int? RequestId { get; set; }

		/// <summary>
		/// Дата доставки заказа, по которому создана задача
		/// Пустая, если у заказа не проставлена дата доставки
		/// </summary>
		public DateTime? OrderDeliveryDate { get; set; }

		/// <summary>
		/// Стадия задачи отправки документа
		/// </summary>
		public DocumentEdoTaskStage? DocumentStage { get; set; }

		/// <summary>
		/// Стадия задачи отправки чека
		/// </summary>
		public EdoReceiptStatus? ReceiptStatus { get; set; }

		/// <summary>
		/// Время создания самой ранней незавершенной итерации трансфера,
		/// по которой перенос кодов еще не запущен
		/// </summary>
		public DateTime? PendingTransferIterationTime { get; set; }

		/// <summary>
		/// Признак наличия незавершенной итерации трансфера,
		/// по которой перенос кодов еще не запущен
		/// </summary>
		public bool HasNotStartedTransfer { get; set; }

		/// <inheritdoc/>
		public DateTime? OutgoingDocumentCreationTime { get; set; }

		/// <inheritdoc/>
		public EdoDocumentStatus? OutgoingDocumentStatus { get; set; }

		/// <inheritdoc/>
		public DateTime? DocflowCreationTime { get; set; }

		/// <inheritdoc/>
		public DateTime? LastActionTime { get; set; }

		/// <inheritdoc/>
		public EdoDocFlowStatus? LastActionState { get; set; }

		/// <inheritdoc/>
		public bool HasProviderAnswer { get; set; }

		/// <inheritdoc/>
		public DateTime? FirstSentActionTime { get; set; }

		/// <inheritdoc/>
		public DateTime? SucceedActionTime { get; set; }

		/// <inheritdoc/>
		public DateTime? WaitingCancellationActionTime { get; set; }

		/// <inheritdoc/>
		public DateTime? CancelledActionTime { get; set; }

		/// <inheritdoc/>
		public TrueMarkTraceabilityStatus? TraceabilityStatus { get; set; }

		/// <inheritdoc/>
		public DateTime? TraceabilityActionTime { get; set; }

		/// <summary>
		/// Стадия последнего фискального документа
		/// </summary>
		public FiscalDocumentStage? FiscalDocumentStage { get; set; }

		/// <summary>
		/// Статус последнего фискального документа
		/// </summary>
		public FiscalDocumentStatus? FiscalDocumentStatus { get; set; }

		/// <summary>
		/// Время последнего изменения состояния фискального документа
		/// </summary>
		public DateTime? FiscalDocumentTime { get; set; }

		/// <summary>
		/// Фискальный номер документа
		/// </summary>
		public string FiscalNumber { get; set; }
	}
}
