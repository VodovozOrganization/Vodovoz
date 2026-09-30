using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Данные для повторной обработки ЭДО проблемы
	/// </summary>
	public class EdoTaskProblemRoutineNode
	{
		/// <summary>
		/// Задача ЭДО
		/// </summary>
		public OrderEdoTask EdoTask { get; set; }

		/// <summary>
		/// Состояние повторной обработки проблемы
		/// </summary>
		public EdoTaskProblemRoutineState RoutineState { get; set; }

		/// <summary>
		/// Проблема задачи ЭДО
		/// </summary>
		public EdoTaskProblem Problem { get; set; }

		/// <summary>
		/// Описание проблемы
		/// </summary>
		public string ProblemDescription { get; set; }

		/// <summary>
		/// Рекомендация
		/// </summary>
		public string Recommendation { get; set; }

		/// <summary>
		/// Id заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Сообщение исключения
		/// </summary>
		public string ExceptionMessage { get; set; }
	}
}
