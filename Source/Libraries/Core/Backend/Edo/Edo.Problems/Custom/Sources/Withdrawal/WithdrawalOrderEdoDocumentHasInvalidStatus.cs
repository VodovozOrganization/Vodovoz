using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources.Withdrawal
{
	public class WithdrawalOrderEdoDocumentHasInvalidStatus : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.WithdrawalOrderEdoDocumentHasInvalidStatus";
		public override string Message => "Документ ЭДО заказа для вывода кодов из оборота имеет недопустимый статус";
		public override string Description => "Проверяет статус документа ЭДО заказа для вывода из оборота";
		public override string Recommendation => "Проверить статус документа ЭДО заказа для вывода из оборота";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
