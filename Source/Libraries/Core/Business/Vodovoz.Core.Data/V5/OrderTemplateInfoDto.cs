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
		/// Идентификатор последнего онлайн заказа по шаблону
		/// </summary>
		public int? LastOnlineOrderIdFromTemplate { get; set; }
		/// <summary>
		/// Список различных сообщений для ИПЗ
		/// </summary>
		public IEnumerable<InfoMessage> InfoMessages { get; set; }

		public static OrderTemplateInfoDto Create(
			OrderTemplateData template,
			string paymentType,
			int? lastOnlineOrderIdFromTemplate,
			IEnumerable<OrderTemplateProductDto> templateProducts,
			decimal orderSum)
		{
			var onlineOrderTemplate = new OrderTemplateInfoDto
			{
				OrderTemplateId = template.OrderTemplateId,
				Weekdays = template.Weekdays,
				DeliveryFrequency = template.DeliveryFrequency,
				PaymentType = paymentType,
				DeliveryAddress = template.DeliveryAddress,
				DeliverySchedule = template.DeliverySchedule,
				IsActive = template.IsActive,
				LastOnlineOrderIdFromTemplate = lastOnlineOrderIdFromTemplate,
				TemplateProducts = templateProducts,
				OrderSum = orderSum,
				InfoMessages = new List<InfoMessage>()
			};

			return onlineOrderTemplate;
		}
	}
}
