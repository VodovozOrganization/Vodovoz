using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Тип события для уведомлений
	/// </summary>
 
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PushNotificationDataEventType
	{
		/// <summary>
		/// Перенос товаров от водителя к водителю
		/// </summary>				
		TranseferAddress,

		/// <summary>
		/// Другое событие изменения состава МЛ
		/// </summary>				
		RouteListContentChanged
	}
}
