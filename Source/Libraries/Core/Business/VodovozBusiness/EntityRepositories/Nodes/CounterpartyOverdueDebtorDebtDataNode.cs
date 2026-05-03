using System;
using System.Diagnostics.Contracts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные по просроченной дебиторской задолженности контрагента
	/// </summary>
	public class CounterpartyOverdueDebtorDebtDataNode
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Контрагент
		/// </summary>
		public Counterparty Counterparty { get; set; }

		/// <summary>
		/// Id организации
		/// </summary>
		public int OrganizationId { get; set; }

		/// <summary>
		/// Полное наименование организации
		/// </summary>
		public string OrganizationFullName { get; set; }

		/// <summary>
		/// Адрес электронной почты организации
		/// </summary>
		public string OrganizationEmailForMailing { get; set; }

		/// <summary>
		/// Договор
		/// </summary>
		public CounterpartyContract Contract { get; set; }

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
