using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Статус адреса маршрутного листа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum RouteListAddressDtoStatus
	{
		/// <summary>
		/// В пути
		/// </summary>
		EnRoute,
		/// <summary>
		/// Завершен
		/// </summary>
		Completed,
		/// <summary>
		/// Отменен
		/// </summary>
		Canceled,
		/// <summary>
		/// Недовоз
		/// </summary>
		Overdue,
		/// <summary>
		/// Перенесен
		/// </summary>
		Transfered
	}
}
