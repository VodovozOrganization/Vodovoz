using Edo.DeviationMonitoring.Validation.Docflow;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources.Transfer
{
	/// <summary>
	/// Документооборот трансфера заведен у провайдера ЭДО, но ответа по нему нет
	/// </summary>
	public class TransferProviderNoResponseValidator : EdoTransferDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TransferProviderNoResponse;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTransferTaskMonitoringNode transferTask) =>
			EdoDocflowDeviationRules.GetNoProviderAnswerTime(transferTask);

		/// <inheritdoc/>
		protected override string BuildDetails(
			EdoTransferTaskMonitoringNode transferTask,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			EdoDocflowDeviationRules.BuildNoProviderAnswerDetails(transferTask, timeout, elapsed);
	}
}
