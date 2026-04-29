using System;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные по просроченной дебиторской задолженности контрагента
	/// </summary>
	public class CounterpartyOverdueDebtorDebtDataNode
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
		/// Id договора
		/// </summary>
		public int ContractId { get; set; }

		/// <summary>
		/// Номер договора
		/// </summary>
		public string ContractNumber { get; set; }

		/// <summary>
		/// Просроченная сумма долга по заказу
		/// </summary>
		public decimal OverdueDebtorDebt { get; set; }

		/// <summary>
		/// Отсрочка дней покупателям
		/// </summary>
		public int CounterpartyPaymentDelayDays { get; set; }

		/// <summary>
		/// Дата доставки заказа
		/// </summary>
		public DateTime OrderDeliveryDate { get; set; }
	}
}
