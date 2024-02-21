using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	/// <summary>
	/// Статус завершенности маршрутного листа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum RouteListDtoCompletionStatus
	{
		/// <summary>
		/// Завершен
		/// </summary>
		Completed,
		/// <summary>
		/// Не завершен
		/// </summary>
		Incompleted
	}
}
