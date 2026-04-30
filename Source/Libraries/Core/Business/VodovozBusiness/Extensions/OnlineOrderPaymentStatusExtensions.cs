using System;
using CustomerOrders.Contracts;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.Extensions
{
	public static class OnlineOrderPaymentStatusExtensions
	{
		public static ExternalOrderPaymentStatus ToExternalOrderPaymentStatus(this OnlineOrderPaymentStatus source)
		{
			switch(source)
			{
				case OnlineOrderPaymentStatus.UnPaid:
					return ExternalOrderPaymentStatus.UnPaid;
				case OnlineOrderPaymentStatus.Paid:
					return ExternalOrderPaymentStatus.Paid;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение статуса онлайн оплаты");
			}
		}
		
		public static OnlineOrderPaymentStatus ToOnlineOrderPaymentStatus(this ExternalOrderPaymentStatus source)
		{
			switch(source)
			{
				case ExternalOrderPaymentStatus.UnPaid:
					return OnlineOrderPaymentStatus.UnPaid;
				case ExternalOrderPaymentStatus.Paid:
					return OnlineOrderPaymentStatus.Paid;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение статуса онлайн оплаты");
			}
		}
		
		public static ExternalOrderPaymentStatus? ToExternalOrderPaymentStatus(this OnlineOrderPaymentStatus? source)
		{
			switch(source)
			{
				case null:
					return null;
				case OnlineOrderPaymentStatus.UnPaid:
					return ExternalOrderPaymentStatus.UnPaid;
				case OnlineOrderPaymentStatus.Paid:
					return ExternalOrderPaymentStatus.Paid;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение статуса онлайн оплаты");
			}
		}
		
		public static OnlineOrderPaymentStatus? ToOnlineOrderPaymentStatus(this ExternalOrderPaymentStatus? source)
		{
			switch(source)
			{
				case null:
					return null;
				case ExternalOrderPaymentStatus.UnPaid:
					return OnlineOrderPaymentStatus.UnPaid;
				case ExternalOrderPaymentStatus.Paid:
					return OnlineOrderPaymentStatus.Paid;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение статуса онлайн оплаты");
			}
		}
	}
}
