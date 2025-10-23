using System;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные по оплатам заказа
	/// </summary>
	public class OrderPaymentsDataNode
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int OrderId { get; set; }
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
		/// Неоплаченная сумма заказа
		/// </summary>
		public decimal NotPaidSum { get; set; }
		/// <summary>
		/// Частично оплаченная сумма заказа
		/// </summary>
		public decimal PartialPaidSum { get; set; }
		/// <summary>
		/// Просроченная сумма долга по заказу
		/// </summary>
		public decimal OverdueDebtorDebt { get; set; }
		/// <summary>
		/// Дата доставки заказа
		/// </summary>
		public DateTime? OrderDeliveryDate { get; set; }
	}
}
