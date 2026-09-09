using Edo.DeviationMonitoring.Validation.Docflow;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources.Transfer
{
	/// <summary>
	/// Документ трансфера создан, но документооборот у провайдера ЭДО не заведен
	/// </summary>
	public class TransferDocumentNotSentToProviderValidator : EdoTransferDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TransferDocumentNotSentToProvider;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTransferTaskMonitoringNode transferTask) =>
			EdoDocflowDeviationRules.GetDocumentNotSentTime(transferTask);

		/// <inheritdoc/>
		protected override string BuildDetails(
			EdoTransferTaskMonitoringNode transferTask,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			EdoDocflowDeviationRules.BuildDocumentNotSentDetails(transferTask, timeout, elapsed);
	}
}
