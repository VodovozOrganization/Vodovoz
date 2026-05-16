using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Carts;
using CustomerOrdersApi.Library.V5.Factories.DeliveryConditions;

namespace CustomerOrdersApi.Library.V5.Factories
{
	/// <summary>
	/// Фабрика доступных методов оплат для сайта ВВ
	/// </summary>
	public class VodovozWebSitePaymentMethodFactory : PaymentMethodFactory
	{
		/// <summary>
		/// Создание методов оплат
		/// </summary>
		/// <returns>Список доступных методов оплат</returns>
		public IEnumerable<PaymentMethod> Create()
		{
			return Create(new []
			{
				(PaymentMethodType.Cash, true),
				(PaymentMethodType.Terminal, true),
				(PaymentMethodType.Online, true),
				(PaymentMethodType.Sbp, true),
				(PaymentMethodType.YandexSplit, true),
			});
		}
	}
}
