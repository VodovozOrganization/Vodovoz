using Vodovoz.Core.Data.InfoMessages;

namespace CustomerOrdersApi.Library.V7.Factories
{
	public class InfoMessageFactory : IInfoMessageFactory
	{
		public InfoMessage CreateNeedPayOrderInfoMessage()
		{
			return InfoMessage.Create("orderDescriptionTop", 2, "Заказ не будет доставлен", "Оплатите заказ в течение {timer}");
		}
		
		public InfoMessage CreateNotPaidOrderInfoMessage()
		{
			return InfoMessage.Create("orderDescriptionTop", 2, "Заказ не был оплачен", "Наш менеджер свяжется с Вами в ближайшее время");
		}

		public InfoMessage CreateRefundPaymentInfoMessage()
		{
			return InfoMessage.Create("cancelOrderPopUp", null, default, "В случае отмены заказа, денежные средства будут возвращены в течение 10 дней");
		}

		public InfoMessage CreatePromoCodeAppliedToNotAllItemsWarning()
		{
			return InfoMessage.Create(null, null, "Скидка применена не ко всем товарам", "Промокод действует не на все позиции в заказе");
		}
		
		public InfoMessage CreateFixedPriceAppliedToNotAllItemsWarning()
		{
			return InfoMessage.Create(null, null, "Фиксированная цена недоступна", "Индивидуальная цена недоступна для одного или нескольких товаров в заказе");
		}
	}
}
