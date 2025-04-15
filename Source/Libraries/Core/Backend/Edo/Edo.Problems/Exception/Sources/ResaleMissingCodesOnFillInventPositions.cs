using Edo.Problems.Exception.EdoExceptions;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.Sources
{
	public class ResaleMissingCodesOnFillInventPositions : EdoTaskProblemExceptionSource
	{
		public override string Name => nameof(ResaleMissingCodesOnFillInventPositionsException);
		public override string Description => "Появляется когда для создания позиции для УПД не удалось найти требуемый код";
		public override string Recommendation => "Проверьте наличие кодов, согласуйте с клиентом возможность замены или отмены заказа. " +
			"Для замены кодов обратитесь за технической поддержкой";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
