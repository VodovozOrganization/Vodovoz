using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Данные для обработки активной проблемы контакта при отправке чека.
	/// </summary>
	public class ReceiptContactProblemNode
	{
		/// <summary>
		/// Задача ЭДО на отправку чека.
		/// </summary>
		public ReceiptEdoTask ReceiptTask { get; set; }

		/// <summary>
		/// Активная проблема контакта.
		/// </summary>
		public EdoTaskProblem Problem { get; set; }

		/// <summary>
		/// Состояние повторной обработки проблемы.
		/// </summary>
		public EdoTaskProblemRoutineState RoutineState { get; set; }

		/// <summary>
		/// Идентификатор заказа.
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Есть ли у задачи коды, уже сохраненные в пул.
		/// </summary>
		public bool HasCodesSavedToPool { get; set; }
	}
}
