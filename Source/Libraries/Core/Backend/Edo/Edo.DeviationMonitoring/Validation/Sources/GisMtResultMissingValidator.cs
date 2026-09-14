using Edo.DeviationMonitoring.Options;
using Edo.DeviationMonitoring.Validation.Docflow;
using Microsoft.Extensions.Options;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// Документооборот завершен, но результат обработки кодов в ГИС МТ не получен
	/// </summary>
	public class GisMtResultMissingValidator : GisMtTaskDeviationValidatorBase
	{
		public GisMtResultMissingValidator(IOptionsSnapshot<EdoDeviationMonitoringOptions> options)
			: base(options)
		{
		}

		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.GisMtResultMissing;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTaskMonitoringNode task) =>
			IsGisMtTracked(task)
				? EdoDocflowDeviationRules.GetGisMtResultMissingTime(task)
				: null;

		/// <inheritdoc/>
		protected override string BuildDetails(EdoTaskMonitoringNode task, TimeSpan timeout, TimeSpan elapsed) =>
			EdoDocflowDeviationRules.BuildGisMtResultMissingDetails(task, timeout, elapsed);
	}
}
