using Edo.Problems.Exception.EdoExceptions;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.Sources
{
	public class ResaleMissingCodes : EdoTaskProblemExceptionSource
	{
		public override string Name => nameof(ResaleMissingCodesException);
		public override string Description => "Проверяет, на все ли товары присутствуют отсканированные коды";
		public override string Recommendation => "Проверьте коды связанные с задачей";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
