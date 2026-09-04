using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.Promotions.Discounts
{
	/// <summary>
	/// Информация для проверки доступности скидки на первый заказ для клиента
	/// </summary>
	public class FirstOrderDiscountConditionsRequestDto
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public Source Source { get; set; }

		/// <summary>
		/// Внешний Id пользователя
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Id пользователя в ДВ
		/// </summary>
		public int? ErpCounterpartyId { get; set; }
	}
}
