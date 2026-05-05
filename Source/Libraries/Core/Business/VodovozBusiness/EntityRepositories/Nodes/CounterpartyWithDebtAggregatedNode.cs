using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Агрегированные данные попросроченной дебиторской задолженности по заказам контрагента
	/// </summary>
	public class CounterpartyWithDebtAggregatedNode
	{
		public Counterparty Counterparty { get; set; }
		public int OrderId { get; set; }
		public decimal Debt { get; set; }
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
	}
}
