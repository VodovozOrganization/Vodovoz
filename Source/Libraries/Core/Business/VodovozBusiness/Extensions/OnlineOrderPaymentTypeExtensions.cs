using System;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;

namespace Vodovoz.Extensions
{
	public static class OnlineOrderPaymentTypeExtensions
	{
		public static PaymentType ToOrderPaymentType(this OnlineOrderPaymentType onlineOrderPaymentType)
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
