using DriverAPI.Library.Models;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Services;

namespace DriverAPI.Library.Converters
{
	public class PaymentTypeConverter
	{
		private readonly IOrderParametersProvider orderParametersProvider;

		public PaymentTypeConverter(IOrderParametersProvider orderParametersProvider)
		{
			this.orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
		}

		public APIPaymentType convertToAPIPaymentType(PaymentType paymentType, Vodovoz.Domain.Orders.PaymentFrom paymentByCardFrom)
		{
			switch (paymentType)
			{
				case PaymentType.cash:
					return APIPaymentType.Cash;
				case PaymentType.cashless:
					return APIPaymentType.Cashless;
				case PaymentType.ByCard:
					if (paymentByCardFrom.Id == orderParametersProvider.PaymentByCardFromSmsId)
					{
						return APIPaymentType.ByCardFromSms;
					}
					else
					{
						return APIPaymentType.ByCard;
					}
				case PaymentType.Terminal:
					return APIPaymentType.Terminal;
				case PaymentType.BeveragesWorld:
				case PaymentType.barter:
				case PaymentType.ContractDoc:
					return APIPaymentType.Payed;
				default:
					throw new ConverterException(nameof(paymentType), paymentType, $"Значение {paymentType} не поддерживается");
			}
		}
	}
}
