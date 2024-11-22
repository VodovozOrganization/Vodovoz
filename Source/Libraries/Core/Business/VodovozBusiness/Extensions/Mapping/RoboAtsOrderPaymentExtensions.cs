using System;
using Vodovoz.Domain.Client;
using Vodovoz.Models.Orders;

namespace VodovozBusiness.Extensions.Mapping
{
	public static class RoboAtsOrderPaymentExtensions
	{
		public static PaymentType MapToPaymentType(this RoboAtsOrderPayment roboAtsOrderPaymentType)
		{
			switch(roboAtsOrderPaymentType)
			{
				case RoboAtsOrderPayment.Cash:
					return PaymentType.Cash;
				case RoboAtsOrderPayment.Terminal:
					return PaymentType.Terminal;
				case RoboAtsOrderPayment.QrCode:
					return PaymentType.DriverApplicationQR;
				default:
					throw new NotSupportedException("Неподдерживаемый тип оплаты через Roboats");
			}
		}
	}
}
