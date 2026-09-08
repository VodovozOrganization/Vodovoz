using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources.Transfer
{
	/// <summary>
	/// Перенос кодов запущен, но не завершается.
	/// Ожидание перемещения кодов в ГИС МТ сюда не относится, для него есть отдельный валидатор
	/// </summary>
	public class TransferTooLongValidator : EdoTransferDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TransferTooLong;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTransferTaskMonitoringNode transferTask)
		{
			if(transferTask.TransferStage != EdoTransferTaskStage.InProgress)
			{
				return null;
			}

			return transferTask.CodesNotMovedProblemTime != null
				? null
				: transferTask.TransferStartTime;
		}

		/// <inheritdoc/>
		protected override string BuildDetails(
			EdoTransferTaskMonitoringNode transferTask,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			$"Перенос кодов запущен "
			+ $"{EdoDeviationTextFormatter.FormatTime(transferTask.TransferStartTime.Value)} "
			+ $"и не завершен за {EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}";
	}
}
