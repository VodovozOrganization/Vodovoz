using Core.Infrastructure;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources.Transfer
{
	/// <summary>
	/// Задача трансфера собрана к отправке, но документ на перенос кодов так и не создан.
	/// Собственной метки у стадий подготовки нет, поэтому отсчет идет
	/// от начала обработки задачи трансфера
	/// </summary>
	public class TransferDocumentNotCreatedValidator : EdoTransferDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TransferDocumentNotCreated;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTransferTaskMonitoringNode transferTask)
		{
			var isPreparingToSend =
				transferTask.TransferStage == EdoTransferTaskStage.PreparingToSend
				|| transferTask.TransferStage == EdoTransferTaskStage.ReadyToSend;

			if(!isPreparingToSend || transferTask.OutgoingDocumentCreationTime != null)
			{
				return null;
			}

			return transferTask.TaskStartTime ?? transferTask.TaskCreationTime;
		}

		/// <inheritdoc/>
		protected override string BuildDetails(
			EdoTransferTaskMonitoringNode transferTask,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			$"Задача трансфера находится на стадии "
			+ $"\"{transferTask.TransferStage.GetEnumDisplayName()}\" "
			+ $"{EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}, "
			+ "документ на перенос кодов не создан";
	}
}
