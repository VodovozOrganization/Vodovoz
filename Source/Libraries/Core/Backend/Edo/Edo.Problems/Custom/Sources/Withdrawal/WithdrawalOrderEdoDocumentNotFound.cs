using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources.Withdrawal
{
	public class WithdrawalOrderEdoDocumentNotFound : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.WithdrawalOrderEdoDocumentNotFound";
		public override string Message => "Документ ЭДО заказа для вывода кодов из оборота не найден";
		public override string Description => "Проверяет наличие документа ЭДО заказа для вывода из оборота";
		public override string Recommendation => "Проверить наличие документа ЭДО заказа для вывода из оборота";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
