using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class WithdrawalTaskHasInvalidCodes : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.WithdrawalTaskHasInvalidCodes";
		public override string Message => "При обработке задачи вывода кодов из оборота обнаружены невалидные коды";
		public override string Description => "Проверяет коды в честном знаке перед выводом из оборота";
		public override string Recommendation => "Проверьте коды связанные с задачей, после исправления повторите вывод из оборота для текущей задачи";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
