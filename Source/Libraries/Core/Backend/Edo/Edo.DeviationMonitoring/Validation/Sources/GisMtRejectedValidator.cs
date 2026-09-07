using Edo.DeviationMonitoring.Options;
using Edo.DeviationMonitoring.Validation.Docflow;
using Microsoft.Extensions.Options;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// ГИС МТ не приняла коды по документообороту.
	/// Условие не про время, поэтому в справочнике этому типу
	/// задается нулевой таймаут: отклонение фиксируется сразу
	/// </summary>
	public class GisMtRejectedValidator : GisMtTaskDeviationValidatorBase
	{
		public GisMtRejectedValidator(IOptionsSnapshot<EdoDeviationMonitoringOptions> options)
			: base(options)
		{
		}

		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.GisMtRejected;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTaskMonitoringNode task) =>
			IsGisMtTracked(task)
				? EdoDocflowDeviationRules.GetGisMtRejectedTime(task)
				: null;

		/// <inheritdoc/>
		protected override string BuildDetails(EdoTaskMonitoringNode task, TimeSpan timeout, TimeSpan elapsed) =>
			EdoDocflowDeviationRules.BuildGisMtRejectedDetails(task);
	}
}
