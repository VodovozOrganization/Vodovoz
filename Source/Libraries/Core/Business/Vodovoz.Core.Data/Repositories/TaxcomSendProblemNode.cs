using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	public class TaxcomSendProblemNode
	{
		/// <summary>
		/// Проблема
		/// </summary>
		public virtual ExceptionEdoTaskProblem Problem { get; set; }

		/// <summary>
		/// ЭДО задача
		/// </summary>
		public virtual OrderEdoTask EdoTask { get; set; }

		/// <summary>
		/// Состояние повторной обработки проблемы
		/// </summary>
		public virtual EdoTaskProblemRoutineState RoutineState { get; set; }
	}
}
