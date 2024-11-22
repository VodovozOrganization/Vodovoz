using System;
using Vodovoz.Domain.Client;
using Vodovoz.Models.Orders;

namespace VodovozBusiness.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="RoboAtsOrderPayment"/>
	/// </summary>
	public static class RoboAtsOrderPaymentExtensions
	{
		/// <summary>
		/// Маппинг запроса на создание заказа в <see cref="PaymentType"/>
		/// </summary>
		/// <param name="roboAtsOrderPaymentType"></param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"></exception>
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
