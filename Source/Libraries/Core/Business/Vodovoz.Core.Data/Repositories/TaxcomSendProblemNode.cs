using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Задача и документ ЭДО с проблемой отправки в Такском.
	/// </summary>
	public class TaxcomSendProblemNode
	{
		/// <summary>
		/// Проблема
		/// </summary>
		public virtual CustomEdoTaskProblem Problem { get; set; }

		/// <summary>
		/// ЭДО задача
		/// </summary>
		public virtual EdoTask EdoTask { get; set; }

		/// <summary>
		/// Состояние повторной обработки проблемы
		/// </summary>
		public virtual EdoTaskProblemRoutineState RoutineState { get; set; }

		/// <summary>
		/// Исходящий документ заказа или трансфера
		/// </summary>
		public virtual OutgoingEdoDocument EdoDocument { get; set; }
	}
}
