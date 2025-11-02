using DriverApi.Contracts.V5;
using DriverApi.Contracts.V5.Responses;
using FastPaymentsApi.Contracts.Responses;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.V5.Converters
{
	/// <summary>
	/// Конвертер оплаты пл QR-коду
	/// </summary>
	public class QrPaymentConverter
	{
		/// <summary>
		/// Метод конвертации в тип оплыты Api
		/// </summary>
		/// <param name="fastPaymentStatus">Статус оплаты СБП</param>
		/// <returns></returns>
		/// <exception cref="ConverterException"></exception>
		public QrPaymentDtoStatus? ConvertToAPIPaymentStatus(FastPaymentStatus? fastPaymentStatus)
		{
			if(fastPaymentStatus == null)
			{
				return null;
			}

			switch(fastPaymentStatus)
			{
				case FastPaymentStatus.Processing:
					return QrPaymentDtoStatus.WaitingForPayment;
				case FastPaymentStatus.Performed:
					return QrPaymentDtoStatus.Paid;
				case FastPaymentStatus.Rejected:
					return QrPaymentDtoStatus.Cancelled;
				default:
					throw new ConverterException(
						nameof(fastPaymentStatus),
						fastPaymentStatus,
						$"Значение {fastPaymentStatus} не поддерживается");
			}
		}

		/// <summary>
		/// Метод конвертации в DTO
		/// </summary>
		/// <param name="qrResponseDto">DTO ответа об оплате по QR-коду</param>
		/// <returns></returns>
		public PayByQrResponse ConvertToPayByQRResponseDto(QRResponseDTO qrResponseDto)
		{
			return new PayByQrResponse
			{
				ErrorMessage = qrResponseDto.ErrorMessage,
				QRCode = qrResponseDto.QRCode,
				QRPaymentStatus = ConvertToAPIPaymentStatus(qrResponseDto.FastPaymentStatus)
			};
		}
	}
}
