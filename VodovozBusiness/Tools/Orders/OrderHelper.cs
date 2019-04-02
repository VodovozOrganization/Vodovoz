using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Tools.Orders
{
	public static class OrderHelper
	{
		public static PaymentType ConvertToPaymentType(PaymentAdapterType type)
		{
			switch(type) 
			{
				case PaymentAdapterType.barter : return PaymentType.barter;
				case PaymentAdapterType.BeveragesWorld: return PaymentType.BeveragesWorld;
				case PaymentAdapterType.ByCard: return PaymentType.ByCard;
				case PaymentAdapterType.cash: return PaymentType.cash;
				case PaymentAdapterType.cashless: return PaymentType.cashless;
				case PaymentAdapterType.ContractDoc: return PaymentType.ContractDoc;
				case PaymentAdapterType.CourierCard: return PaymentType.ByCard;
				default: return PaymentType.cash;
			}
		}

		public static PaymentType ConvertToPaymentType(this Order order)
		{
			return ConvertToPaymentType(order.PaymentAdapterType);
		}

		public static PaymentAdapterType ConvertToPaymentAdapterType(PaymentType type)
		{
			switch(type) {
				case PaymentType.barter: return PaymentAdapterType.barter;
				case PaymentType.BeveragesWorld: return PaymentAdapterType.BeveragesWorld;
				case PaymentType.ByCard: return PaymentAdapterType.ByCard;
				case PaymentType.cash: return PaymentAdapterType.cash;
				case PaymentType.cashless: return PaymentAdapterType.cashless;
				case PaymentType.ContractDoc: return PaymentAdapterType.ContractDoc;
				default: return PaymentAdapterType.cash;
			}
		}

		public static PaymentAdapterType ConvertToPaymentAdapterType(this Order order)
		{
			if(order.PaymentType == PaymentType.ByCard && order.OnlineOrder == null)
				return PaymentAdapterType.CourierCard;
			return ConvertToPaymentAdapterType(order.PaymentType);
		}
	}
}
