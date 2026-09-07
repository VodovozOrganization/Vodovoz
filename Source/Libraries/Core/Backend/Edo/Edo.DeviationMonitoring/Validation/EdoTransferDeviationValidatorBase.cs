using System;
using Vodovoz.Core.Data.Repositories;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Базовый валидатор отклонения по задаче трансфера.
	/// Заявки у трансфера свои и к заявке ЭДО заказа отношения не имеют,
	/// поэтому отклонение привязывается только к задаче
	/// </summary>
	public abstract class EdoTransferDeviationValidatorBase
		: EdoDeviationValidatorBase<EdoTransferTaskMonitoringNode>, IEdoTransferDeviationValidator
	{
		/// <inheritdoc/>
		protected override void FillEntity(
			EdoDeviationValidationResult result,
			EdoTransferTaskMonitoringNode transferTask)
		{
			result.EdoTaskId = transferTask.EdoTaskId;
			result.EdoRequestId = null;
			result.StageName = EdoDeviationTextFormatter.GetTransferStageName(transferTask);
		}
	}
}
