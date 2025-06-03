using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Requests.V1
{
	/// <summary>
	/// Подписание документов (для юр. лиц)
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum SignatureType
	{
		/// <summary>
		/// Печать
		/// </summary>
		BySeal,
		/// <summary>
		/// Доверенность
		/// </summary>
		ByProxy,
		/// <summary>
		/// Доверенность на точке доставки
		/// </summary>
		ProxyOnDeliveryPoint
	}
}
