namespace Vodovoz.Core.Data.InfoMessages
{
	public static class Messages
	{
		public static class OutOfStock
		{
			public const string ItemOutOfStock = "Товар закончился :(";
			public const string SomeProductsUnavailableToOrder = "Некоторые товары в корзине недоступны для заказа";
			public const string AllProductsUnavailableToOrder = "Все товары в корзине недоступны для заказа";
		}
		
		public static class PriceChanged
		{
			public const string Title = "Цена изменилась";
			public const string Description =  "Цена на некоторые товары в Вашей корзине изменилась";
		}
		
		public static class Unavailable
		{
			public const string PromoSet = "Промонабор недоступен";
			public const string PromoCode = "Промокод недоступен";
			public const string PromoSetForNewClientOnlyForNewClients = "Промонаборы для новых клиентов доступны только для новых клиентов";
			public const string PromoCodeUnavailable =  "Данный промокод больше недоступен";
		}
		
		public static class DeliveryChanged
		{
			public const string Title = "Платная доставка";
		}
	}
}
