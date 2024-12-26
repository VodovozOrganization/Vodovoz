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
		/// Передача заказов между водителями
		/// </summary>				
		TransferAddress,

		/// <summary>
		/// Другое событие изменения состава МЛ
		/// </summary>				
		RouteListContentChanged
	}
}
