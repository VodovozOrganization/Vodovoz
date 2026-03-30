using System;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Агрегированные данные по неоплаченным безналичным заказам контрагента
	/// </summary>
	public class CounterpartyOrdersAggregatedNode
	{
		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Id организации
		/// </summary>
		public int OrganizationId { get; set; }

		/// <summary>
		/// Наименование организации
		/// </summary>
		public string OrganizationName { get; set; }

		/// <summary>
		/// Суммарная неоплаченная сумма по заказам контрагента
		/// </summary>
		public decimal TotalNotPaidSum { get; set; }

		/// <summary>
		/// Суммарная частично оплаченная сумма по заказам контрагента
		/// </summary>
		public decimal TotalPartialPaidSum { get; set; }

		/// <summary>
		/// Суммарная просроченная сумма долга по заказам контрагента
		/// </summary>
		public decimal TotalOverdueDebtorDebt { get; set; }

		/// <summary>
		/// Минимальная дата доставки среди неоплаченных заказов контрагента
		/// </summary>
		public DateTime? MinOrderDeliveryDate { get; set; }
	}
}
