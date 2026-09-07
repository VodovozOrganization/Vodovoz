using Core.Infrastructure;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// Задача создана, но обработчик к ней так и не приступил:
	/// она дольше допустимого висит в статусе <see cref="EdoTaskStatus.New"/>,
	/// и никакой проблемы по ней не заведено.
	/// <para>
	/// Проверяется именно статус самой задачи, а не стадия документооборота:
	/// стадия остается начальной и у задачи, которую обработчик уже взял в работу,
	/// а здесь ловится ровно тот случай, когда обработка не начиналась в принципе.
	/// Задача с проблемой сюда не попадает: причина остановки по ней уже известна,
	/// и меряет ее не мониторинг отклонений
	/// </para>
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
