using Vodovoz.Core.Data.Repositories;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Валидатор отклонения по заявке ЭДО, для которой еще не создана задача
	/// </summary>
	public interface IEdoRequestDeviationValidator : IEdoDeviationValidator<EdoRequestMonitoringNode>
	{
	}
}
