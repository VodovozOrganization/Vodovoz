using DriverApi.Contracts.V5;
using Vodovoz.Domain.Client;

namespace DriverAPI.Library.V5.Converters
{
	/// <summary>
	/// Конвертер типов оплаты
	/// </summary>
	public class PaymentTypeConverter
	{
		/// <summary>
		/// Метод конвертации в DTO
		/// </summary>
		/// <param name="paymentType">Тип оплаты</param>
		/// <param name="paid">Оплачен ли</param>
		/// <returns></returns>
		/// <exception cref="ConverterException"></exception>
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
