using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources.Withdrawal
{
	public class WithdrawalCanBeCreatedOnlyForCashlessOrders : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.WithdrawalCanBeCreatedOnlyForCashlessOrders";
		public override string Message => "Документ вывода кодов из оборота может быть создан только для заказов по безналу";
		public override string Description => "Проверяет форму оплаты заказа перед выводом из оборота";
		public override string Recommendation => "Проверьте форму оплаты заказа";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
