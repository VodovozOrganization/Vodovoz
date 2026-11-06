using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Контекст замены кода маркировки для отдельной позиции чека
	/// </summary>
	public class PositionCodeReplacementContext
	{
		/// <summary>
		/// Позиция фискального документа
		/// </summary>
		public FiscalInventPosition Position { get; set; }

		/// <summary>
		/// Сущность кода продукта задачи
		/// </summary>
		public TrueMarkProductCode ProductCode { get; set; }

		/// <summary>
		/// Новый код маркировки, подобранный из пула
		/// </summary>
		public TrueMarkWaterIdentificationCode NewCode { get; set; }
	}
}
