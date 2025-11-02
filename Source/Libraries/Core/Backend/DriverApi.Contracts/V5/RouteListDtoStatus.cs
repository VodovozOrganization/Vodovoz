using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Статус маршрутного листа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum RouteListDtoStatus
	{
		/// <summary>
		/// Новый
		/// </summary>
		New,
		/// <summary>
		/// Подтвержден
		/// </summary>
		Confirmed,
		/// <summary>
		/// На погрузке
		/// </summary>
		InLoading,
		/// <summary>
		/// В пути
		/// </summary>
		EnRoute,
		/// <summary>
		/// Доставлен
		/// </summary>
		Delivered,
		/// <summary>
		/// На закрытии
		/// </summary>
		OnClosing,
		/// <summary>
		/// Проверка километража
		/// </summary>
		MileageCheck,
		/// <summary>
		/// Закрыт
		/// </summary>
		Closed
	}
}
