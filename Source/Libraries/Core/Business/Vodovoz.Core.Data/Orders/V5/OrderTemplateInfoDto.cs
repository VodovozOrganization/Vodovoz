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
		//TODO 5695:Когда станет известным точный состав автозаказа, тогда надо доделать его наполнение
		public IEnumerable<OnlineOrderItemDto> OrderItems { get; set; }
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
			IEnumerable<OnlineOrderItemDto> orderItems,
			decimal orderSum)
		{
			var onlineOrderTemplate = new OrderTemplateInfoDto
			{
				OrderTemplateId = template.Id,
				Weekday = template.Weekday,
				RepeatOrder = template.RepeatOrder,
				PaymentType = template.PaymentType,
				DeliveryAddress = deliveryAddress,
				DeliverySchedule = deliverySchedule,
				IsActive = template.IsActive,
				LastOnlineOrderExternalId = lastOnlineOrderExternalId,
				OrderItems = orderItems,
				OrderSum = orderSum
			};

			return onlineOrderTemplate;
		}
	}
}
