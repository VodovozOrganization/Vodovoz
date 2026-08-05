using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Данные для обработки активной проблемы нехватки кодов в пуле
	/// </summary>
	public class CodePoolMissingProblemNode
	{
		/// <summary>
		/// Проблема
		/// </summary>
		public ExceptionEdoTaskProblem Problem { get; set; }

		/// <summary>
		/// Связанная ЭДО задача
		/// </summary>
		public OrderEdoTask EdoTask { get; set; }

		/// <summary>
		/// Состояние повторной обработки проблемы
		/// </summary>
		public EdoTaskProblemRoutineState RoutineState { get; set; }
	}
}
