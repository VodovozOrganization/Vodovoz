using DriverAPI.Library.DTOs;
using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.Converters
{
	/// <summary>
	/// Конвертер оплаты пл QR-коду
	/// </summary>
	public class QRPaymentConverter
	{
		/// <summary>
		/// Метод конвертации в тип оплыты Api
		/// </summary>
		/// <param name="fastPaymentStatus">Статус оплаты СБП</param>
		/// <returns></returns>
		/// <exception cref="ConverterException"></exception>
		public QRPaymentDTOStatus? ConvertToAPIPaymentStatus(FastPaymentStatus? fastPaymentStatus)
		{
			if(fastPaymentStatus == null)
			{
				return null;
			}

			switch(fastPaymentStatus)
			{
				case FastPaymentStatus.Processing:
					return QRPaymentDTOStatus.WaitingForPayment;
				case FastPaymentStatus.Performed:
					return QRPaymentDTOStatus.Paid;
				case FastPaymentStatus.Rejected:
					return QRPaymentDTOStatus.Cancelled;
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
		public PayByQRResponseDTO ConvertToPayByQRResponseDto(QRResponseDTO qrResponseDto)
		{
			return new PayByQRResponseDTO
			{
				ErrorMessage = qrResponseDto.ErrorMessage,
				QRCode = qrResponseDto.QRCode,
				QRPaymentStatus = ConvertToAPIPaymentStatus(qrResponseDto.FastPaymentStatus)
			};
		}
	}
}
