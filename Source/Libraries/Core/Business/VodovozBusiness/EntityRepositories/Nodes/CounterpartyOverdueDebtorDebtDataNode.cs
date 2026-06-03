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
		/// Id Контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

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
		/// Идентификатор договора
		/// </summary>
		public int ContractId { get; set; }

		/// <summary>
		/// Просроченная сумма долга по заказу
		/// </summary>
		public decimal OverdueDebtorDebt { get; set; }
	}
}
