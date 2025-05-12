using System.Text.Json.Serialization;

namespace WarehouseApi.Contracts.Dto.V1
{
	/// <summary>
	/// Уровень кода маркировки ЧЗ
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum WarehouseApiTruemarkCodeLevel
	{
		/// <summary>
		/// Транспортный код
		/// </summary>
		transport,
		/// <summary>
		/// Групповой код
		/// </summary>
		group,
		/// <summary>
		/// Единичный код
		/// </summary>
		unit
	}
}
