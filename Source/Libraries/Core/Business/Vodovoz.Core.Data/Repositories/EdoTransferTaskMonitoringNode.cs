using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Состояние задачи трансфера, достаточное для проверки условий отклонений
	/// сервисом мониторинга документооборота
	/// </summary>
	public class EdoTransferTaskMonitoringNode : IEdoDocflowMonitoringNode
	{
		/// <summary>
		/// Код задачи трансфера
		/// </summary>
		public int EdoTaskId { get; set; }

		/// <summary>
		/// Статус задачи трансфера
		/// </summary>
		public EdoTaskStatus TaskStatus { get; set; }

		/// <summary>
		/// Стадия переноса кодов
		/// </summary>
		public EdoTransferTaskStage TransferStage { get; set; }

		/// <summary>
		/// Время создания задачи трансфера
		/// </summary>
		public DateTime TaskCreationTime { get; set; }

		/// <summary>
		/// Время начала обработки задачи трансфера.
		/// От него воркер трансферов отсчитывает ожидание заявок
		/// </summary>
		public DateTime? TaskStartTime { get; set; }

		/// <summary>
		/// Время начала переноса кодов: задача отправлена в ЭДО
		/// </summary>
		public DateTime? TransferStartTime { get; set; }

		/// <summary>
		/// Признак того, что задача завершена или отменена.
		/// Аннулирование задачи сюда не входит: это еще идущий процесс,
		/// который может зависнуть, и мониторинг должен его видеть
		/// </summary>
		public bool IsFinished { get; set; }

		/// <summary>
		/// Признак наличия активной зарегистрированной проблемы по задаче,
		/// кроме ожидания перемещения кодов: его мониторинг меряет сам
		/// </summary>
		public bool HasActiveProblem { get; set; }

		/// <summary>
		/// Время регистрации незакрытой проблемы ожидания перемещения кодов в ГИС МТ.
		/// Пустое, если такой проблемы нет
		/// </summary>
		public DateTime? CodesNotMovedProblemTime { get; set; }

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
	}
}
