using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V7
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
