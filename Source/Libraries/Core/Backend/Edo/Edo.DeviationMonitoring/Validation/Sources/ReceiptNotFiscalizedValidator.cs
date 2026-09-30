using Core.Infrastructure;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// Чек отправлен в кассу, но не фискализирован
	/// </summary>
	public class ReceiptNotFiscalizedValidator : EdoTaskDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.ReceiptNotFiscalized;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTaskMonitoringNode task)
		{
			if(task.TaskType != EdoTaskType.Receipt)
			{
				return null;
			}

			if(task.FiscalDocumentStatus is null)
			{
				// задача переведена в отправку, но фискального документа еще нет
				// отсчитываем стадию от времени начала обработки задачи
				return task.ReceiptStatus == EdoReceiptStatus.Sending ? task.TaskStartTime : null;
			}

			var isFinished =
				task.FiscalDocumentStage == FiscalDocumentStage.Completed
				|| task.FiscalDocumentStatus == FiscalDocumentStatus.Printed
				|| task.FiscalDocumentStatus == FiscalDocumentStatus.Completed
				|| !string.IsNullOrEmpty(task.FiscalNumber);

			var isWaitingCallback = task.FiscalDocumentStatus == FiscalDocumentStatus.WaitForCallback;

			return isFinished || isWaitingCallback ? null : task.FiscalDocumentTime;
		}

		/// <inheritdoc/>
		protected override string BuildDetails(EdoTaskMonitoringNode task, TimeSpan timeout, TimeSpan elapsed)
		{
			if(task.FiscalDocumentStatus is null)
			{
				return $"Чек передан в отправку {EdoDeviationTextFormatter.FormatTime(task.TaskStartTime.Value)}, "
					+ $"фискальный документ не создан за "
					+ $"{EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}";
			}

			return $"Фискальный документ находится в статусе "
				+ $"\"{task.FiscalDocumentStatus.Value.GetEnumDisplayName()}\" "
				+ $"с {EdoDeviationTextFormatter.FormatTime(task.FiscalDocumentTime.Value)}, "
				+ $"это {EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}";
		}
	}
}
