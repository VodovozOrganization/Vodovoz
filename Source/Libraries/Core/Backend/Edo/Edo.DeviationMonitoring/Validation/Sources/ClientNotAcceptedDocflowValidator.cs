using Edo.DeviationMonitoring.Validation.Docflow;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// Документ получен оператором, но клиент не завершает документооборот.
	/// К этому же случаю относится статус "В процессе": им провайдер ЭДО сообщает,
	/// что документ у контрагента и ждет подписи
	/// </summary>
	public class ClientNotAcceptedDocflowValidator : EdoTaskDeviationValidatorBase
	{
		/// <inheritdoc/>
		public override EdoDeviationType DeviationType => EdoDeviationType.ClientNotAcceptedDocflow;

		/// <inheritdoc/>
		protected override DateTime? GetStageStartTime(EdoTaskMonitoringNode task) =>
			EdoDocflowDeviationRules.GetClientNotAcceptedTime(task);

		/// <inheritdoc/>
		protected override string BuildDetails(EdoTaskMonitoringNode task, TimeSpan timeout, TimeSpan elapsed) =>
			$"Документ получен оператором {EdoDeviationTextFormatter.FormatTime(task.FirstSentActionTime.Value)}, "
			+ $"клиент не завершил документооборот за "
			+ $"{EdoDeviationTextFormatter.FormatElapsed(elapsed, timeout)}";
	}
}
