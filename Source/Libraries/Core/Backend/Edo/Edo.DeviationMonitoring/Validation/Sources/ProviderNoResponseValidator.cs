using Edo.DeviationMonitoring.Validation.Docflow;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// Документооборот заведен у провайдера ЭДО, но ни одного ответа по нему не получено
	/// </summary>
	public class ProviderNoResponseValidator : EdoTaskDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.ProviderNoResponse;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTaskMonitoringNode task) =>
			EdoDocflowDeviationRules.GetNoProviderAnswerTime(task);

		/// <inheritdoc/>
		protected override string BuildDetails(EdoTaskMonitoringNode task, TimeSpan timeout, TimeSpan elapsed) =>
			EdoDocflowDeviationRules.BuildNoProviderAnswerDetails(task, timeout, elapsed);
	}
}
