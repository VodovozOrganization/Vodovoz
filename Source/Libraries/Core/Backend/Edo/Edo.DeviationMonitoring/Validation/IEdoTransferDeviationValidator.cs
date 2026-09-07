using Vodovoz.Core.Data.Repositories;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Валидатор отклонения по задаче трансфера.
	/// Трансфер проверяется отдельным семейством валидаторов: у него свой набор стадий
	/// и свой документооборот, хотя УПД трансфера уходит тем же трактом, что и УПД заказа
	/// </summary>
	public interface IEdoTransferDeviationValidator : IEdoDeviationValidator<EdoTransferTaskMonitoringNode>
	{
		/// <summary>
		/// Признак того, что проверка имеет смысл и для завершенной задачи.
		/// Принятый документ трансфера завершает задачу, а результат обработки кодов
		/// в ГИС МТ приходит уже после этого
		/// </summary>
		bool IsAppliesToFinishedTask { get; }
	}
}
