using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Extensions
{
	public static class OnlineOrderPaymentTypeExtensions
	{
		public static PaymentType ConvertToOrderPaymentType(this OnlineOrderPaymentType onlineOrderPaymentType)
		{
			switch(onlineOrderPaymentType)
			{
				case OnlineOrderPaymentType.Cash:
					return PaymentType.Cash;
				case OnlineOrderPaymentType.Terminal:
					return PaymentType.Terminal;
				case OnlineOrderPaymentType.PaidOnline:
					return PaymentType.PaidOnline;
				default:
					throw new InvalidOperationException("Неизвестный тип онлайн оплаты");
			}
		}
	}
}
