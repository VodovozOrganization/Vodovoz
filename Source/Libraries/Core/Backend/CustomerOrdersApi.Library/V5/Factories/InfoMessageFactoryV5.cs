using Vodovoz.Core.Data.InfoMessages;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;

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
				"orderDescriptionTop",
				2,
				null,
				"Подключая {OrderTemplateLabel}, вы экономите время и до {Discount} на покупках!",
				new []
				{
					InfoMessageParameter.Create(
						"OrderTemplateLabel",
						"автозаказ",
						InfoMessageParameterAction.Create("OpenModal", "OrderTemplateInfo")),
					InfoMessageParameter.Create("Discount", discount + units.GetEnumDisplayName())
				});
		}
	}
}
