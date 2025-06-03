using Vodovoz.Domain.Orders;
using Vodovoz.RobotMia.Contracts.Requests.V1;

namespace Vodovoz.RobotMia.Api.Extensions.Mapping
{
	/// <summary>
	/// Методы расширения для маппинга типов подписи
	/// </summary>
	public static class SignatureTypeExtensions
	{
		/// <summary>
		/// Маппинг типов подписи из RobotMiaApi в Vodovoz
		/// </summary>
		/// <param name="signatureType"></param>
		/// <returns></returns>
		public static OrderSignatureType? MapToVodovozSignatureType(this SignatureType? signatureType)
			=> signatureType switch
			{
				SignatureType.BySeal => (OrderSignatureType?)OrderSignatureType.BySeal,
				SignatureType.ByProxy => (OrderSignatureType?)OrderSignatureType.ByProxy,
				SignatureType.ProxyOnDeliveryPoint => (OrderSignatureType?)OrderSignatureType.ProxyOnDeliveryPoint,
				_ => null,
			};
	}
}
