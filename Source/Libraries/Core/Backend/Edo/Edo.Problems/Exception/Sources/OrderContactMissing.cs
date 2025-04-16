using Edo.Common;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.Sources
{
	public class OrderContactMissing : EdoTaskProblemExceptionSource
	{
		public override string Name => nameof(OrderContactMissingException);
		public override string Description => "Появляется когда не найден ни один подходящий контакт в заказе";
		public override string Recommendation => "Заполните необходимые контакты";
		public override EdoProblemImportance Importance => EdoProblemImportance.Waiting;
	}
}
