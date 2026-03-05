using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;

namespace Vodovoz.Core.Data.Orders.V5
{
	public class OrderTemplateInfoDto : OrderTemplateDto
	{
		/// <summary>
		/// Адрес доставки
		/// </summary>
		public string DeliveryAddress { get; set; }
		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<OrderTemplateProductDto> TemplateProducts { get; set; }
		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum { get; set; }
		/// <summary>
		/// Тип оплаты
		/// </summary>
		public OnlineOrderPaymentType PaymentType { get; set; }
		/// <summary>
		/// Интервал доставки в формате с 07:00 до 16:00
		/// </summary>
		public string DeliverySchedule { get; set; }
		/// <summary>
		/// Идентификатор последнего онлайн заказа
		/// </summary>
		public Guid? LastOnlineOrderExternalId { get; set; }

		public static OrderTemplateInfoDto Create(
			OnlineOrderTemplate template,
			string deliveryAddress,
			string deliverySchedule,
			Guid? lastOnlineOrderExternalId,
			IEnumerable<OrderTemplateProductDto> templateProducts,
			decimal orderSum)
		{
			var onlineOrderTemplate = new OrderTemplateInfoDto
			{
				OrderTemplateId = template.Id,
				Weekdays = template.Weekdays,
				RepeatOrder = template.RepeatOrder,
				PaymentType = template.PaymentType,
				DeliveryAddress = deliveryAddress,
				DeliverySchedule = deliverySchedule,
				IsActive = template.IsActive,
				LastOnlineOrderExternalId = lastOnlineOrderExternalId,
				TemplateProducts = templateProducts,
				OrderSum = orderSum
			};

			return onlineOrderTemplate;
		}
	}
}
