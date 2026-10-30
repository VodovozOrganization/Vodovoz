using System;
using CustomerOrders.Contracts.V8.Sale.Templates;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V8.Extensions
{
	public static class TemplatePaymentTypeExtensions
	{
		public static TemplatePaymentType ToTemplatePaymentType(this OnlineOrder onlineOrder)
		{
			switch(onlineOrder.OnlineOrderPaymentType)
			{
				case OnlineOrderPaymentType.Cash:
					return TemplatePaymentType.Cash;
				case OnlineOrderPaymentType.Terminal:
					return TemplatePaymentType.Terminal;
				case OnlineOrderPaymentType.PaidOnline:
					if(onlineOrder.OnlinePaymentSource
						is OnlinePaymentSource.FromAiBotByQr
						or OnlinePaymentSource.FromMobileAppByQr
						or OnlinePaymentSource.FromVodovozWebSite)
					{
						return TemplatePaymentType.Sbp;
					}

					return TemplatePaymentType.Online;
				default:
					throw new ArgumentOutOfRangeException(
						nameof(OnlineOrderPaymentType),
						onlineOrder.OnlineOrderPaymentType.ToString(),
						"Данное значение онлайн оплаты не поддерживается для перевода в тип оплаты автозаказа");
			}
		}

		public static TemplatePaymentType ToTemplatePaymentType(this OnlineOrderPaymentType source)
		{
			switch(source)
			{
				case OnlineOrderPaymentType.Cash:
					return TemplatePaymentType.Cash;
				case OnlineOrderPaymentType.Terminal:
					return TemplatePaymentType.Terminal;
				case OnlineOrderPaymentType.PaidOnline:
					return TemplatePaymentType.Online;
				default:
					throw new ArgumentOutOfRangeException(
						nameof(OnlineOrderPaymentType),
						source.ToString(),
						"Неизвестный тип оплаты онлайн заказа");
			}
		}
	}
}
