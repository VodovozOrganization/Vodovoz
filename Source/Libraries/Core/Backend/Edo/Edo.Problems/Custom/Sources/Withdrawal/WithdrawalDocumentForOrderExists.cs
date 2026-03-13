using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources.Withdrawal
{
	public class WithdrawalDocumentForOrderExists : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.WithdrawalDocumentForOrderExists";
		public override string Message => "Документ вывода из оборота кодов ЧЗ заказа уже создан";
		public override string Description => "Проверяет наличие сохраненного документа вывода из оборота по заказу";
		public override string Recommendation => "Проверьте наличие сохраненного документа вывода из оборота по заказу";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
