using Edo.DeviationMonitoring.Options;
using Edo.DeviationMonitoring.Validation.Docflow;
using Microsoft.Extensions.Options;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources.Transfer
{
	/// <summary>
	/// ГИС МТ не приняла коды по документообороту трансфера
	/// </summary>
	public class TransferGisMtRejectedValidator : GisMtTransferDeviationValidatorBase
	{
		public TransferGisMtRejectedValidator(IOptionsSnapshot<EdoDeviationMonitoringOptions> options)
			: base(options)
		{
		}

		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TransferGisMtRejected;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTransferTaskMonitoringNode transferTask) =>
			IsGisMtTracked(transferTask)
				? EdoDocflowDeviationRules.GetGisMtRejectedTime(transferTask)
				: null;

		/// <inheritdoc/>
		protected override string BuildDetails(
			EdoTransferTaskMonitoringNode transferTask,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			EdoDocflowDeviationRules.BuildGisMtRejectedDetails(transferTask);
	}
}
