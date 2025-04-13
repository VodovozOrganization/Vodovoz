using Edo.Problems.Custom;
using System.Runtime.Serialization;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.Sources
{
	public class MissingCodeInPool : EdoTaskProblemExceptionSource
	{
		public override string Name => nameof(EdoCodePoolMissingCodeException);
		public override string Description => "В пуле не хватает кодов";
		public override string Recommendation => "Добавьте коды в пул";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
