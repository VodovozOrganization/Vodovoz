using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.V7.Dto.Orders.OrderItem;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V8.Dto.Sale
{
	/// <summary>
	/// Запрос на расчёт баланса, доступного списания и начисления бонусов при оформлении заказа
	/// </summary>
	public class ApplyBonusToOrderRequest
	{
		/// <summary>
		/// ИПЗ
		/// </summary>
		public Source Source { get; set; }

		/// <summary>
		/// Идентификатор заказа из ИПЗ
		/// </summary>
		public Guid ExternalOrderId { get; set; }

		/// <summary>
		/// Идентификатор клиента в ERP
		/// </summary>
		public int? ErpCounterpartyId { get; set; }

		/// <summary>
		/// Идентификатор клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Сколько бонусов клиент хочет списать. 0 — только расчёт. Допустимы только целые значения
		/// </summary>
		public decimal? BonusesToWriteOff { get; set; }

		/// <summary>
		/// Позиции корзины до списания бонусов
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> OnlineOrderItems { get; set; }
	}
}
