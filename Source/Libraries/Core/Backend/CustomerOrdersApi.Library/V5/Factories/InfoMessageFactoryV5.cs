using Vodovoz.Core.Data.InfoMessages;

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
		
		//TODO 5695: внести изменения согласно задачи
		public InfoMessage CreateTemplateDiscountInfoMessage()
		{
			return InfoMessage.Create(
				"orderDescriptionTop",
				2,
				null, 
				"Подключая {AutoOrderLabel}, вы экономите время и до {discount}% на покупках!");
		}
	}
}
