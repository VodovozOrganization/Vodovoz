using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Данные для применения скидки по первому заказу из ИПЗ
	/// </summary>
	public class CanApplyFirstOrderDiscountRequest
	{
		/// <summary>
		/// Источник
		/// </summary>
		public Source Source { get; set; }

		/// <summary>
		/// Id клиента
		/// </summary>
		public int? CounterpartyId { get; set; }

		/// <summary>
		/// Идентификатор пользователя ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Товары онлайн заказа
		/// </summary>
		public IEnumerable<IOrderedCartItem> CartItems { get; set; }

		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum => CartItems.Sum(x => x.CurrentSum);

		public static CanApplyFirstOrderDiscountRequest Create(
			Source source,
			int? counterpartyId,
			Guid? externalCounterpartyId,
			IEnumerable<IOrderedCartItem> cartItems) =>
			new CanApplyFirstOrderDiscountRequest
			{
				Source = source,
				CounterpartyId = counterpartyId,
				ExternalCounterpartyId = externalCounterpartyId,
				CartItems = cartItems
			};
	}
}
