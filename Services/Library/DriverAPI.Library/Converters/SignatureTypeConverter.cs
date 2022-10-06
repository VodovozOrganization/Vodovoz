using DriverAPI.Library.DTOs;
using Vodovoz.Domain.Orders;

namespace DriverAPI.Library.Converters
{
	public class SignatureTypeConverter
	{
		public SignatureDtoType? ConvertToApiSignatureType(OrderSignatureType? signatureType)
		{
			if (signatureType == null)
			{
				return null;
			}

			switch (signatureType)
			{
				case OrderSignatureType.ByProxy:
					return SignatureDtoType.ByProxy;
				case OrderSignatureType.BySeal:
					return SignatureDtoType.BySeal;
				case OrderSignatureType.ProxyOnDeliveryPoint:
					return SignatureDtoType.ProxyOnDeliveryPoint;
				case OrderSignatureType.SignatureTranscript:
					return SignatureDtoType.SignatureTranscript;
				default:
					throw new ConverterException(nameof(signatureType), signatureType, $"Значение {signatureType} не поддерживается");
			}
		}
	}
}
