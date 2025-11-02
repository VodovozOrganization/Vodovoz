using Edo.Common;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.Sources
{
	public class CodeDuplicated : EdoTaskProblemExceptionSource
	{
		public override string Name => nameof(CodeDuplicatedException);
		public override string Description => "Появляется когда производится попытка сохранить код который уже есть в системе в другом заказе";
		public override string Recommendation => "Согласуйте с клиентом изменение заказа или отмените заказ";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
