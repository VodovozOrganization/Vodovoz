using DriverAPI.Library.DTOs;
using Vodovoz.Domain.Client;

namespace DriverAPI.Library.Converters
{
	public class PaymentTypeConverter
	{
		public PaymentDtoType ConvertToAPIPaymentType(PaymentType paymentType, bool paid, PaymentByTerminalSource? paymentByTerminalSource)
		{
			switch(paymentType)
			{
				case PaymentType.Cash:
					return PaymentDtoType.Cash;
				case PaymentType.Terminal:
					if(paymentByTerminalSource != null && paymentByTerminalSource == PaymentByTerminalSource.ByQR)
					{
						return PaymentDtoType.TerminalQR;
					}
					return PaymentDtoType.TerminalCard;
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
