using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Агрегированные данные попросроченной дебиторской задолженности по заказам контрагента
	/// </summary>
	public class CounterpartyWithDebtAggregatedNode
	{
		/// <summary>
		/// Контрагент
		/// </summary>
		public Counterparty Counterparty { get; set; }

		/// <summary>
		/// Id заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Просроченная задолженность
		/// </summary>
		public decimal Debt { get; set; }

		/// <summary>
		/// Статус оплаты заказа
		/// </summary>
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
	}
}
