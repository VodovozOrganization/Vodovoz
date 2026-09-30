using Edo.DeviationMonitoring.Validation.Docflow;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// Исходящий документ создан, но документооборот у провайдера ЭДО не заведен
	/// </summary>
	public class DocumentNotSentToProviderValidator : EdoTaskDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.DocumentNotSentToProvider;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTaskMonitoringNode task) =>
			EdoDocflowDeviationRules.GetDocumentNotSentTime(task);

		/// <inheritdoc/>
		protected override string BuildDetails(EdoTaskMonitoringNode task, TimeSpan timeout, TimeSpan elapsed) =>
			EdoDocflowDeviationRules.BuildDocumentNotSentDetails(task, timeout, elapsed);
	}
}
