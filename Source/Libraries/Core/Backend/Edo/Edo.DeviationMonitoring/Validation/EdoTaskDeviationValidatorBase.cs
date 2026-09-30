using System;
using Vodovoz.Core.Data.Repositories;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Базовый валидатор отклонения по задаче отправки документа или чека
	/// </summary>
	public abstract class EdoTaskDeviationValidatorBase
		: EdoDeviationValidatorBase<EdoTaskMonitoringNode>, IEdoTaskDeviationValidator
	{
		/// <inheritdoc/>
		protected override void FillEntity(EdoDeviationValidationResult result, EdoTaskMonitoringNode task)
		{
			result.EdoTaskId = task.EdoTaskId;
			result.EdoRequestId = task.RequestId;
			result.StageName = EdoDeviationTextFormatter.GetStageName(task);
		}
	}
}
