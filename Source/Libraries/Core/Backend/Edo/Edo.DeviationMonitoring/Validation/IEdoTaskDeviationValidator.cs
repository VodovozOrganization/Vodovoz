using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Валидатор отклонения по задаче ЭДО
	/// </summary>
	public interface IEdoTaskDeviationValidator : IEdoDeviationValidator<EdoTaskMonitoringNode>
	{
		/// <summary>
		/// Признак того, что проверка имеет смысл и для завершенной задачи
		/// </summary>
		bool IsAppliesToFinishedTask { get; }
	}
}
