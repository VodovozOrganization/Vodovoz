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
		/// Признак того, что проверка имеет смысл и для завершенной задачи.
		/// <para>
		/// Обычное отклонение — это застрявшая обработка, поэтому завершение задачи
		/// его снимает. Исключение — результат обработки кодов в ГИС МТ:
		/// он приходит уже после того, как завершение документооборота
		/// перевело задачу в <see cref="EdoTaskStatus.Completed"/>
		/// </para>
		/// </summary>
		bool IsAppliesToFinishedTask { get; }
	}
}
