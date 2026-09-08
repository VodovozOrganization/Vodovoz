using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// Итерация трансфера создана, но перенос кодов не запущен
	/// </summary>
	public class TransferNotStartedValidator : EdoTaskDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TransferNotStarted;

		/// <summary>
		/// Проверяет, применим ли валидатор к состоянию задачи
		/// </summary>
		/// <param name="task">Состояние задачи ЭДО</param>
		public override bool IsApplicable(EdoTaskMonitoringNode task)
		{
			if(task is null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			return IsTransfering(task);
		}

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTaskMonitoringNode task)
		{
			if(!IsTransfering(task))
			{
				return null;
			}

			return task.HasNotStartedTransfer ? task.PendingTransferIterationTime : null;
		}

		/// <inheritdoc/>
		protected override string BuildDetails(EdoTaskMonitoringNode task, TimeSpan timeout, TimeSpan elapsed) =>
			$"Заявки на трансфер созданы {EdoDeviationTextFormatter.FormatTime(task.PendingTransferIterationTime.Value)}, "
			+ $"перенос кодов не запущен "
			+ $"{EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}";

		private static bool IsTransfering(EdoTaskMonitoringNode task) =>
			task.DocumentStage == DocumentEdoTaskStage.Transfering
			|| task.ReceiptStatus == EdoReceiptStatus.Transfering;
	}
}
