using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources.Withdrawal
{
	public class WithdrawalCanBeCreatedOnlyForOwnNeedsOrders : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.WithdrawalCanBeCreatedOnlyForOwnNeedsOrders";
		public override string Message => "Документ вывода кодов из оборота может быть создан только для заказов для собственных нужд";
		public override string Description => "Проверяет цель покупки перед выводом из оборота";
		public override string Recommendation => "Проверьте цель покупки в карточке контрагента";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
