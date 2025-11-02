using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Статус приема
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum AcceptanceStatus
	{
		/// <summary>
		/// Не принят
		/// </summary>
		NotAccepted,
		/// <summary>
		/// Принят
		/// </summary>
		Accepted
	}
}
