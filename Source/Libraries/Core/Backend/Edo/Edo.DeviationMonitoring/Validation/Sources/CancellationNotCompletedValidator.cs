using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// Документооборот ожидает аннулирования дольше допустимого
	/// </summary>
	public class CancellationNotCompletedValidator : EdoTaskDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.CancellationNotCompleted;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTaskMonitoringNode task) =>
			task.LastActionState == EdoDocFlowStatus.WaitingForCancellation
				? task.WaitingCancellationActionTime
				: null;

		/// <inheritdoc/>
		protected override string BuildDetails(EdoTaskMonitoringNode task, TimeSpan timeout, TimeSpan elapsed) =>
			$"Документооборот ожидает аннулирования с "
			+ $"{EdoDeviationTextFormatter.FormatTime(task.WaitingCancellationActionTime.Value)}, "
			+ $"это {EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}";
	}
}
