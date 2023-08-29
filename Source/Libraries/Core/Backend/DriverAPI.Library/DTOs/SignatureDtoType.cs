using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	/// <summary>
	/// Подписание документов
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum SignatureDtoType
	{
		/// <summary>
		/// По печати
		/// </summary>
		BySeal,
		/// <summary>
		/// По доверенности
		/// </summary>
		ByProxy,
		/// <summary>
		/// По доверенности на точке доставки
		/// </summary>
		ProxyOnDeliveryPoint,
		/// <summary>
		/// Подпись клиента
		/// </summary>
		SignatureTranscript
	}
}
