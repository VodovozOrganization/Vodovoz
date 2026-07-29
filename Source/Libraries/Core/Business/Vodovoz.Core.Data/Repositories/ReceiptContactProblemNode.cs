using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Данные для обработки активной проблемы контакта при отправке чека.
	/// </summary>
	public class ReceiptContactProblemNode
	{
		public ReceiptEdoTask ReceiptTask { get; set; }
		public EdoTaskProblem Problem { get; set; }
		public EdoTaskProblemRoutineState RoutineState { get; set; }
		public int OrderId { get; set; }
		public bool HasCodesSavedToPool { get; set; }
	}
}
