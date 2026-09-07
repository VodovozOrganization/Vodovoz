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
		/// Стадия трансфера считается опознанной на всем ее протяжении, а не только
		/// пока перенос не запущен: длительность запущенного переноса меряют отклонения
		/// по самой задаче трансфера, и резервному валидатору здесь делать нечего.
		/// Иначе одна медленная задача трансфера дала бы в журнале
		/// по строке "задача зависла" на каждую задачу-инициатора
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

		/// <summary>
		/// Задача ждет переноса кодов
		/// </summary>
		private static bool IsTransfering(EdoTaskMonitoringNode task) =>
			task.DocumentStage == DocumentEdoTaskStage.Transfering
			|| task.ReceiptStatus == EdoReceiptStatus.Transfering;
	}
}
