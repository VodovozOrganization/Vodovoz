using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V6
{
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum DriverApiTruemarkCodeLevel
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
