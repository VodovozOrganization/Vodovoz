using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// Резервный валидатор: задача не завершена дольше допустимого,
	/// при этом ни одно частное условие не сработало.
	/// Нужно, чтобы за границы мониторинга не уходили неизвестные проблемы отправки.
	/// <para>
	/// Резервным его делает положение в перечислении: <see cref="EdoDeviationType.TaskStalled"/>
	/// объявлен последним, а сервис фиксирует первое сработавшее отклонение,
	/// поэтому до этого валидатора очередь доходит, только если не сработали остальные
	/// </para>
	/// </summary>
	public class TaskStalledValidator : EdoTaskDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TaskStalled;

		/// <summary>
		/// Резервный валидатор: работает, только когда к задаче
		/// неприменим ни один частный валидатор
		/// </summary>
		public override bool IsFallback => true;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTaskMonitoringNode task) =>
			task.TaskCreationTime;

		/// <inheritdoc/>
		protected override string BuildDetails(EdoTaskMonitoringNode task, TimeSpan timeout, TimeSpan elapsed) =>
			$"Задача создана {EdoDeviationTextFormatter.FormatTime(task.TaskCreationTime)} "
			+ $"и не завершена {EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}. "
			+ $"Текущая стадия \"{EdoDeviationTextFormatter.GetStageName(task)}\", "
			+ "причина задержки не определена";
	}
}
