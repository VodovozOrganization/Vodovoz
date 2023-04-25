using DriverAPI.Library.Converters;
using DriverAPI.Library.Deprecated.DTOs;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Services;

namespace DriverAPI.Library.Deprecated.Converters
{
	[Obsolete("Будет удален с прекращением поддержки API v1")]
	public class PaymentTypeConverter
	{
		private readonly IOrderParametersProvider _orderParametersProvider;

		public PaymentTypeConverter(IOrderParametersProvider orderParametersProvider)
		{
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
		}

		public PaymentDtoType ConvertToAPIPaymentType(PaymentType paymentType, Vodovoz.Domain.Orders.PaymentFrom paymentByCardFrom)
		{
			switch(paymentType)
			{
				case PaymentType.Cash:
					return PaymentDtoType.Cash;
				case PaymentType.Cashless:
					return PaymentDtoType.Cashless;
				case PaymentType.PaidOnline:
					if(paymentByCardFrom.Id == _orderParametersProvider.PaymentByCardFromSmsId)
					{
						return PaymentDtoType.ByCardFromSms;
					}
					else
					{
						return PaymentDtoType.ByCard;
					}
				case PaymentType.TerminalQR:
					return PaymentDtoType.Terminal;
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
