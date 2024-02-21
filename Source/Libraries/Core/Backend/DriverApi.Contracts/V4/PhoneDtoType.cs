using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V4
{
	/// <summary>
	/// Тип телефона
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PhoneDtoType
	{
		/// <summary>
		/// Телефон точки доставки
		/// </summary>
		DeliveryPoint,
		/// <summary>
		/// Телефон контрагента
		/// </summary>
		Counterparty
	}
}
