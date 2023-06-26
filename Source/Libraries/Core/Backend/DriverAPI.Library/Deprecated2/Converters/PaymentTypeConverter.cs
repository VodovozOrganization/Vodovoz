using DriverAPI.Library.Deprecated2.DTOs;
using DriverAPI.Library.Converters;
using System;
using Vodovoz.Domain.Client;

namespace DriverAPI.Library.Deprecated2.Converters
{
	public class PaymentTypeConverter
	{
		[Obsolete("Будет удален с прекращением поддержки API v2")]
		public PaymentDtoType ConvertToAPIPaymentType(PaymentType paymentType, bool paid)
		{
			switch(paymentType)
			{
				case PaymentType.Cash:
					return PaymentDtoType.Cash;
				case PaymentType.Terminal:
					return PaymentDtoType.Terminal;
				case PaymentType.DriverApplicationQR:
					return PaymentDtoType.DriverApplicationQR;
				case PaymentType.SmsQR:
					if(paid)
					{
						return PaymentDtoType.Paid;
					}
					else
					{
						return PaymentDtoType.DriverApplicationQR;
					}
				case PaymentType.PaidOnline:
				case PaymentType.Barter:
				case PaymentType.ContractDocumentation:
					return PaymentDtoType.Paid;
				case PaymentType.Cashless:
					return PaymentDtoType.Cashless;
				default:
					throw new ConverterException(nameof(paymentType), paymentType, $"Значение {paymentType} не поддерживается");
			}
		}
	}
}
