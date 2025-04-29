using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class ResaleHasInvalidCodesOnTransferComplete : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.ResaleHasInvalidCodesOnTransferComplete";
		public override string Message => "При перепродаже обнаружены не валидные коды на стадии трансфера";
		public override string Description => "Проверяет коды в честном знаке после завершения трансфера для перепродажи";
		public override string Recommendation => "Проверьте коды связанные с задачей, после исправления повторите завершение трансфера для текущей задачи";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
