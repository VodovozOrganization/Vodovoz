using DriverApi.Contracts.V6;
using Vodovoz.Domain;

namespace DriverAPI.Library.V6.Converters
{
	/// <summary>
	/// Конвертер статуса оплаты по смс
	/// </summary>
	public class SmsPaymentStatusConverter
	{
		/// <summary>
		/// Метод конвертации в DTO
		/// </summary>
		/// <param name="smsPaymentStatus">Статус оплаты по смс ДВ</param>
		/// <returns></returns>
		/// <exception cref="ConverterException"></exception>
		public SmsPaymentDtoStatus? ConvertToAPIPaymentStatus(SmsPaymentStatus? smsPaymentStatus)
		{
			if(smsPaymentStatus == null)
			{
				return null;
			}

			switch(smsPaymentStatus)
			{
				case SmsPaymentStatus.WaitingForPayment:
					return SmsPaymentDtoStatus.WaitingForPayment;
				case SmsPaymentStatus.Paid:
					return SmsPaymentDtoStatus.Paid;
				case SmsPaymentStatus.Cancelled:
					return SmsPaymentDtoStatus.Cancelled;
				default:
					throw new ConverterException(nameof(smsPaymentStatus), smsPaymentStatus, $"Значение {smsPaymentStatus} не поддерживается");
			}
		}
	}
}
