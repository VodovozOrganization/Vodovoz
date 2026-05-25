using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Carts;

namespace CustomerOrdersApi.Library.V5.Factories.DeliveryConditions
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
				(nameof(PaymentMethodType.Cash), true),
				(nameof(PaymentMethodType.Terminal), true),
				(nameof(PaymentMethodType.Online), true),
				(nameof(PaymentMethodType.Sbp), true),
				(nameof(PaymentMethodType.YandexSplit), true)
			});
		}
	}
}
