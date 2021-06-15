using DriverAPI.Library.DTOs;
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

		public PaymentDtoType convertToAPIPaymentType(PaymentType paymentType, Vodovoz.Domain.Orders.PaymentFrom paymentByCardFrom)
		{
			switch (paymentType)
			{
				case PaymentType.cash:
					return PaymentDtoType.Cash;
				case PaymentType.cashless:
					return PaymentDtoType.Cashless;
				case PaymentType.ByCard:
					if (paymentByCardFrom.Id == orderParametersProvider.PaymentByCardFromSmsId)
					{
						return PaymentDtoType.ByCardFromSms;
					}
					else
					{
						return PaymentDtoType.ByCard;
					}
				case PaymentType.Terminal:
					return PaymentDtoType.Terminal;
				case PaymentType.BeveragesWorld:
				case PaymentType.barter:
				case PaymentType.ContractDoc:
					return PaymentDtoType.Payed;
				default:
					throw new ConverterException(nameof(paymentType), paymentType, $"Значение {paymentType} не поддерживается");
			}
		}
	}
}
