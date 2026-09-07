using Core.Infrastructure;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources.Transfer
{
	/// <summary>
	/// Резервный валидатор трансфера: задача не завершена дольше допустимого,
	/// при этом ни одно частное условие не сработало.
	/// <para>
	/// Резервным его делает положение в перечислении: <see cref="EdoDeviationType.TransferStalled"/>
	/// объявлен последним в трансферном блоке, а сервис фиксирует первое сработавшее отклонение
	/// </para>
	/// </summary>
	public class TransferStalledValidator : EdoTransferDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TransferStalled;

		/// <summary>
		/// Резервный валидатор: работает, только когда к задаче трансфера
		/// неприменим ни один частный валидатор
		/// </summary>
		public override bool IsFallback => true;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTransferTaskMonitoringNode transferTask) =>
			transferTask.TaskCreationTime;

		/// <inheritdoc/>
		protected override string BuildDetails(
			EdoTransferTaskMonitoringNode transferTask,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			$"Задача трансфера создана "
			+ $"{EdoDeviationTextFormatter.FormatTime(transferTask.TaskCreationTime)} "
			+ $"и не завершена {EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}. "
			+ $"Текущая стадия \"{transferTask.TransferStage.GetEnumDisplayName()}\", "
			+ "причина задержки не определена";
	}
}
