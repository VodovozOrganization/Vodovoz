using System;
using System.Collections.Generic;
using TaxcomEdo.Contracts.Counterparties;

namespace TaxcomEdo.Contracts.Orders
{
	/// <summary>
	/// Информация о заказе для ЭДО(электронного документооборота)
	/// </summary>
	public class OrderInfoForEdo
	{
		/// <summary>
		/// Id заказа
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Номер документа строкой (для использования новой нумерации документов)
		/// </summary>
		public string StringNumber { get; set; }
		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string CounterpartyExternalOrderId { get; set; }
		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime DeliveryDate { get; set; }
		/// <summary>
		/// Дата создания заказа
		/// </summary>
		public DateTime CreationDate { get; set; }
		/// <summary>
		/// Информация о контракте <see cref="CounterpartyContractInfoForEdo"/>
		/// </summary>
		public CounterpartyContractInfoForEdo ContractInfoForEdo { get; set; }
		/// <summary>
		/// Информация о клиенте <see cref="CounterpartyInfoForEdo"/>
		/// </summary>
		public CounterpartyInfoForEdo CounterpartyInfoForEdo { get; set; }
		/// <summary>
		/// Информация о ТД <see cref="DeliveryPointInfoForEdo"/>
		/// </summary>
		public DeliveryPointInfoForEdo DeliveryPointInfoForEdo { get; set; }
		/// <summary>
		/// Информация о товарах заказа <see cref="OrderItemInfoForEdo"/>
		/// </summary>
		public IList<OrderItemInfoForEdo> OrderItems { get; set; }
	}
}
