using Edo.DeviationMonitoring.Options;
using Edo.DeviationMonitoring.Validation.Docflow;
using Microsoft.Extensions.Options;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources.Transfer
{
	/// <summary>
	/// Документооборот трансфера завершен, но результат обработки кодов в ГИС МТ не получен
	/// </summary>
	public class TransferGisMtResultMissingValidator : GisMtTransferDeviationValidatorBase
	{
		public TransferGisMtResultMissingValidator(IOptionsSnapshot<EdoDeviationMonitoringOptions> options)
			: base(options)
		{
		}

		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.TransferGisMtResultMissing;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTransferTaskMonitoringNode transferTask) =>
			IsGisMtTracked(transferTask)
				? EdoDocflowDeviationRules.GetGisMtResultMissingTime(transferTask)
				: null;

		/// <inheritdoc/>
		protected override string BuildDetails(
			EdoTransferTaskMonitoringNode transferTask,
			TimeSpan timeout,
			TimeSpan elapsed) =>
			EdoDocflowDeviationRules.BuildGisMtResultMissingDetails(transferTask, timeout, elapsed);
	}
}
