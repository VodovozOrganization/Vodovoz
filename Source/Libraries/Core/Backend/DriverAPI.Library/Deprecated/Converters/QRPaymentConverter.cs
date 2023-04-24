using DriverAPI.Library.Converters;
using DriverAPI.Library.DTOs;
using System;
using Vodovoz.Domain.FastPayments;
using PayByQRResponseDTO = DriverAPI.Library.Deprecated.DTOs.PayByQRResponseDTO;

namespace DriverAPI.Library.Deprecated.Converters
{
	[Obsolete("Будет удален с прекращением поддержки API v1")]
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
