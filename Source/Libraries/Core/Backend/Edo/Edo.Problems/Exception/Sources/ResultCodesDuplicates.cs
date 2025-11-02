using Edo.Common;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.Sources
{
	public class ResultCodesDuplicates : EdoTaskProblemExceptionSource
	{
		public override string Name => nameof(ResultCodesDuplicatesException);
		public override string Description => "Появляется когда в задаче присутствует несколько одинаковых Result кодов";
		public override string Recommendation => "Обратитесь за технической поддержкой";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
