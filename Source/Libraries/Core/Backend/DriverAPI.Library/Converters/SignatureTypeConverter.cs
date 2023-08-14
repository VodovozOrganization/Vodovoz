using DriverAPI.Library.DTOs;
using Vodovoz.Domain.Orders;

namespace DriverAPI.Library.Converters
{
	/// <summary>
	/// Конвертер типа подписания заказа
	/// </summary>
	public class SignatureTypeConverter
	{
		/// <summary>
		/// Метод конвертации в DTO
		/// </summary>
		/// <param name="signatureType">Подписание документов</param>
		/// <returns></returns>
		/// <exception cref="ConverterException"></exception>
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
