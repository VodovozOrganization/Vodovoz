using System;
using System.Collections.Generic;

namespace Vodovoz.Core.Data.V5
{
	public class OrderTemplateInfoDto : OrderTemplateData
	{
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
		public string PaymentType { get; set; }
		/// <summary>
		/// Идентификатор последнего онлайн заказа
		/// </summary>
		public Guid? LastOnlineOrderExternalId { get; set; }
		/// <summary>
		/// Список различных сообщений для ИПЗ
		/// </summary>
		public IEnumerable<InfoMessage> InfoMessages { get; set; }

		public static OrderTemplateInfoDto Create(
			OrderTemplateData template,
			string paymentType,
			Guid? lastOnlineOrderExternalId,
			IEnumerable<OrderTemplateProductDto> templateProducts,
			decimal orderSum)
		{
			var onlineOrderTemplate = new OrderTemplateInfoDto
			{
				OrderTemplateId = template.OrderTemplateId,
				Weekdays = template.Weekdays,
				RepeatOrder = template.RepeatOrder,
				PaymentType = paymentType,
				DeliveryAddress = template.DeliveryAddress,
				DeliverySchedule = template.DeliverySchedule,
				IsActive = template.IsActive,
				LastOnlineOrderExternalId = lastOnlineOrderExternalId,
				TemplateProducts = templateProducts,
				OrderSum = orderSum,
				InfoMessages = new List<InfoMessage>()
			};

			return onlineOrderTemplate;
		}
	}
}
