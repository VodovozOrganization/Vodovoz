using System;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Organizations;

namespace TaxcomEdo.Contracts.OrdersWithoutShipment
{
	/// <summary>
	/// Информация о счете без погрузки для ЭДО(электронного документооборота)
	/// </summary>
	public abstract class OrderWithoutShipmentInfo
	{
		/// <summary>
		/// Id счета без погрузки
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Дата создания
		/// </summary>
		public DateTime CreationDate { get; set; }
		/// <summary>
		/// Информация об организации
		/// </summary>
		public OrganizationInfoForEdo OrganizationInfoForEdo { get; set; }
		/// <summary>
		/// Информация о клиенте
		/// </summary>
		public CounterpartyInfoForEdo CounterpartyInfoForEdo { get; set; }
		/// <summary>
		/// Сумма
		/// </summary>
		public decimal Sum { get; set; }
		/// <summary>
		/// Номер счета
		/// </summary>
		public virtual string BillNumber => $"Ф-{Id}";
	}
}
