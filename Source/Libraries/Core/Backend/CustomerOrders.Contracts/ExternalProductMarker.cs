using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts
{
	/// <summary>
	/// Акция в ИПЗ
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ExternalProductMarker
	{
		/// <summary>
		/// Товар недели
		/// </summary>
		ProductOfWeek,
		/// <summary>
		/// Скидка
		/// </summary>
		Sale
	}
}
