using DriverAPI.Library.Deprecated.DTOs;
using DriverAPI.Library.DTOs;
using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.Converters
{
	public class QRPaymentConverter
	{
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
