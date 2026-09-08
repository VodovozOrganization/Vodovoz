using Core.Infrastructure;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// Задача создана, но обработчик к ней так и не приступил
	/// и никакой проблемы по ней не заведено
	/// </summary>
	public class TaskNotStartedValidator : EdoTaskDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TaskNotStarted;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTaskMonitoringNode task)
		{
			var isNotStarted = task.TaskStatus == EdoTaskStatus.New && !task.HasActiveProblem;

			return isNotStarted ? task.TaskCreationTime : (DateTime?)null;
		}

		/// <inheritdoc/>
		protected override string BuildDetails(EdoTaskMonitoringNode task, TimeSpan timeout, TimeSpan elapsed) =>
			$"Задача создана {EdoDeviationTextFormatter.FormatTime(task.TaskCreationTime)} "
			+ $"и остается в статусе \"{task.TaskStatus.GetEnumDisplayName()}\" "
			+ $"{EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}. "
			+ "Обработчик не приступал к обработке задачи";
	}
}
