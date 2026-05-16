using System;
using CustomerOrders.Contracts;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Extensions
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
		
		public static ExternalOrderPaymentType? ToExternalOrderPaymentType(this OnlineOrderPaymentType? source)
		{
			switch(source)
			{
				case null:
					return null;
				case OnlineOrderPaymentType.Cash:
					return ExternalOrderPaymentType.Cash;
				case OnlineOrderPaymentType.Terminal:
					return ExternalOrderPaymentType.Terminal;
				case OnlineOrderPaymentType.PaidOnline:
					return ExternalOrderPaymentType.PaidOnline;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение формы онлайн оплаты");
			}
		}
		
		public static ExternalOrderPaymentType ToExternalOrderPaymentType(this OnlineOrderPaymentType source)
		{
			switch(source)
			{
				case OnlineOrderPaymentType.Cash:
					return ExternalOrderPaymentType.Cash;
				case OnlineOrderPaymentType.Terminal:
					return ExternalOrderPaymentType.Terminal;
				case OnlineOrderPaymentType.PaidOnline:
					return ExternalOrderPaymentType.PaidOnline;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение формы онлайн оплаты");
			}
		}
		
		public static OnlineOrderPaymentType? ToOnlineOrderPaymentType(this ExternalOrderPaymentType? source)
		{
			switch(source)
			{
				case null:
					return null;
				case ExternalOrderPaymentType.Cash:
					return OnlineOrderPaymentType.Cash;
				case ExternalOrderPaymentType.Terminal:
					return OnlineOrderPaymentType.Terminal;
				case ExternalOrderPaymentType.PaidOnline:
					return OnlineOrderPaymentType.PaidOnline;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение формы онлайн оплаты");
			}
		}
		
		public static OnlineOrderPaymentType ToOnlineOrderPaymentType(this ExternalOrderPaymentType source)
		{
			switch(source)
			{
				case ExternalOrderPaymentType.Cash:
					return OnlineOrderPaymentType.Cash;
				case ExternalOrderPaymentType.Terminal:
					return OnlineOrderPaymentType.Terminal;
				case ExternalOrderPaymentType.PaidOnline:
					return OnlineOrderPaymentType.PaidOnline;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение формы онлайн оплаты");
			}
		}
	}
}
