using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using InfoMessage = CustomerOrders.Contracts.InfoMessages.InfoMessage;

namespace CustomerOrdersApi.Library.V5.Factories
{
	public class InfoMessageFactoryV5 : IInfoMessageFactoryV5
	{
		public InfoMessage CreateNeedPayOrderInfoMessage()
		{
			return InfoMessage.Create("orderDescriptionTop", 2, "Заказ не будет доставлен", "Оплатите заказ в течение {timer}");
		}

		public InfoMessage CreateNotPaidOrderInfoMessage()
		{
			return InfoMessage.Create("orderDescriptionTop", 2, "Заказ не был оплачен", "Наш менеджер свяжется с Вами в ближайшее время");
		}

		public InfoMessage CreateAutoOrderDiscountInfoMessage(decimal discount, DiscountUnits units)
		{
			return InfoMessage.Create(
				"autoOrderBottom",
				2,
				$"{discount}{units.GetEnumDisplayName()} скидка при автозаказе",
				$"Подключая автозаказ, Вы экономите время и до {discount}{units.GetEnumDisplayName()} на покупках!");
		}
	}
}
