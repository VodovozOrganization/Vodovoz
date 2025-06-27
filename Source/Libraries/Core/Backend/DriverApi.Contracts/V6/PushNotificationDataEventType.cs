using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Тип события для уведомлений
	/// </summary>
 
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PushNotificationDataEventType
	{
		/// <summary>
		/// Передача заказов между водителями из свободных остатков
		/// </summary>				
		TransferAddressFromHandToHand,

		/// <summary>
		/// Другое событие изменения состава МЛ
		/// </summary>				
		RouteListContentChanged
	}
}
