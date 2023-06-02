using DriverAPI.Library.Converters;
using DriverAPI.Library.Deprecated.DTOs;
using System;
using Vodovoz.Domain.Client;

namespace DriverAPI.Library.Deprecated.Converters
{
	[Obsolete("Будет удален с прекращением поддержки API v1")]
	public class PaymentTypeConverter
	{
		public PaymentDtoType ConvertToAPIPaymentType(PaymentType paymentType, bool paid)
		{
			switch(paymentType)
			{
				case PaymentType.Cash:
					return PaymentDtoType.Cash;
				case PaymentType.Cashless:
					return PaymentDtoType.Cashless;
				case PaymentType.PaidOnline:
					return PaymentDtoType.ByCard; //В прошлой версии отсутствует "оплачено" заглушено через по карте
				case PaymentType.DriverApplicationQR: //В прошлой версии отсутствует оплата через МП заглушено через терминал
				case PaymentType.Terminal:
					return PaymentDtoType.Terminal;
				case PaymentType.SmsQR:
					if(paid)
					{
						return PaymentDtoType.ByCardFromSms; //В прошлой версии отсутствует "оплачено" заглушено через по карте по смс
					}
					else
					{
						return PaymentDtoType.Terminal; //В прошлой версии отсутствует оплата через МП заглушено через терминал
					}
				case PaymentType.Barter:
					return PaymentDtoType.Barter;
				case PaymentType.ContractDocumentation:
					return PaymentDtoType.ContractDocumentation;
				default:
					throw new ConverterException(nameof(paymentType), paymentType, $"Значение {paymentType} не поддерживается");
			}
		}
	}
}
