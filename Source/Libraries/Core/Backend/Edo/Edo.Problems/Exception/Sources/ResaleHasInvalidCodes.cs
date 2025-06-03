using Edo.Common;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.Sources
{
	public class ResaleHasInvalidCodes : EdoTaskProblemExceptionSource
	{
		public override string Name => nameof(ResaleHasInvalidCodesException);
		public override string Description => "Появляется если в задаче обнаружены коды не прошедшие проверку в честном знаке";
		public override string Recommendation => "Проверьте коды в задаче";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
