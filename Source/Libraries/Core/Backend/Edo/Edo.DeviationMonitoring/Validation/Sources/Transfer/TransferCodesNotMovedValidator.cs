using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources.Transfer
{
	/// <summary>
	/// Документооборот трансфера завершен, но коды так и не сменили владельца в ГИС МТ.
	/// Такие задачи переспрашивает воркер трансферов, а по задаче висит незакрытая
	/// проблема ожидания перемещения: отклонение фиксирует, что ожидание затянулось
	/// </summary>
	public class TransferCodesNotMovedValidator : EdoTransferDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TransferCodesNotMoved;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTransferTaskMonitoringNode transferTask) =>
			transferTask.CodesNotMovedProblemTime;

		/// <inheritdoc/>
		protected override string BuildDetails(
			EdoTransferTaskMonitoringNode transferTask,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			$"Документооборот трансфера завершен, но с "
			+ $"{EdoDeviationTextFormatter.FormatTime(transferTask.CodesNotMovedProblemTime.Value)} "
			+ $"коды не сменили владельца в ГИС МТ. Это "
			+ $"{EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}";
	}
}
