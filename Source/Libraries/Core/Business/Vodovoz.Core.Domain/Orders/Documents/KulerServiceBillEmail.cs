using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	internal static class KulerServiceBillEmail
	{
		public static bool IsOrder(OrderEntity order, IOrganizationSettings organizationSettings) =>
			order?.Contract?.Organization?.Id == organizationSettings?.KulerServiceOrganizationId;

		public static EmailTemplate Create(OrderEntity order)
		{
			var orderDate = order.DeliveryDate ?? order.BillDate;
			var datePart = orderDate != default ? $" от {orderDate:dd.MM.yy}" : string.Empty;
			var intro = $"Для Вас сформирован актуальный счет для оплаты заказа № {order.Id}{datePart}.";

			return new EmailTemplate
			{
				Title = "ООО Кулер-Сервис",
				Text =
					"Здравствуйте!\n" +
					intro + "\n" +
					"Мы стремимся стать лучше и хотим еще больше радовать наших клиентов! Поэтому нам так важно услышать ваше мнение. Расскажите, что нам следовало бы изменить в нашей работе, и мы обязательно учтем ваши рекомендации.\n" +
					"Вы можете оформить заказ в любое удобное время. Мы работаем 24 часа и 7 дней в неделю.\n" +
					"Спасибо, что Вы с нами.\n" +
					"С Уважением,\n" +
					"Команда компании \"Кулер-Сервис\".\n\n" +
					"тел.: +7 (812) 317-07-97\n" +
					"тел. моб.: + 7 (969) 726-92-84\n" +
					"_____________________________________________\n" +
					"Наш официальный сайт: кулерсервис.рф",
				TextHtml =
					"<p>Здравствуйте!</p>\n" +
					$"<p>{intro}</p>\n" +
					"<p>Мы стремимся стать лучше и хотим еще больше радовать наших клиентов! Поэтому нам так важно услышать ваше мнение. Расскажите, что нам следовало бы изменить в нашей работе, и мы обязательно учтем ваши рекомендации.</p>\n" +
					"<p>Вы можете оформить заказ в любое удобное время. Мы работаем 24 часа и 7 дней в неделю.</p>\n" +
					"<p>Спасибо, что Вы с нами.</p>\n" +
					"<p>С Уважением,</p>\n" +
					"<p>Команда компании \"Кулер-Сервис\".</p>\n" +
					"<p>тел.: +7 (812) 317-07-97</p>\n" +
					"<p>тел. моб.: + 7 (969) 726-92-84</p>\n" +
					"<p>_____________________________________________</p>\n" +
					"<p>Наш официальный сайт: кулерсервис.рф</p>\n"
			};
		}
	}
}
