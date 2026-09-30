using Vodovoz.Core.Data.Repositories;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Валидатор отклонения по задаче трансфера
	/// </summary>
	public interface IEdoTransferDeviationValidator : IEdoDeviationValidator<EdoTransferTaskMonitoringNode>
	{
		/// <summary>
		/// Признак того, что проверка имеет смысл и для завершенной задачи
		/// </summary>
		bool IsAppliesToFinishedTask { get; }
	}
}
