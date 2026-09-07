using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources.Transfer
{
	/// <summary>
	/// Задача трансфера собирает заявки дольше допустимого.
	/// Досылку залежавшихся задач делает воркер трансферов, поэтому срабатывание
	/// означает, что не работает он сам: очередь, воркер или его настройка ожидания.
	/// Таймаут в справочнике должен быть заметно больше настройки ожидания заявок
	/// </summary>
	public class TransferWaitingRequestsTooLongValidator : EdoTransferDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TransferWaitingRequestsTooLong;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTransferTaskMonitoringNode transferTask) =>
			transferTask.TransferStage == EdoTransferTaskStage.WaitingRequests
				? transferTask.TaskStartTime ?? transferTask.TaskCreationTime
				: (DateTime?)null;

		/// <inheritdoc/>
		protected override string BuildDetails(
			EdoTransferTaskMonitoringNode transferTask,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			$"Задача трансфера создана "
			+ $"{EdoDeviationTextFormatter.FormatTime(transferTask.TaskCreationTime)} "
			+ $"и ждет заявок {EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}. "
			+ "Досылка залежавшихся трансферов не сработала";
	}
}
