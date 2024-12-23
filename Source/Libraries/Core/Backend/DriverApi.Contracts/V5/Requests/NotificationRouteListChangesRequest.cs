using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V5.Requests
{
	/// <summary>
	/// Запрос на уведомление водителя об изменения в МЛ
	/// </summary>

	public class NotificationRouteListChangesRequest
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		[Required]
		public int OrderId { get; set; }

		/// <summary>
		/// Тип события для уведомлений
		/// </summary>
		[Required]
		public PushNotificationDataEventType? PushNotificationDataEventType { get; set; }
	}
}
