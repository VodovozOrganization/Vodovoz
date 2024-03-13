namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Уведомление<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#notification"/>
	/// </summary>
	public class Notification
	{
		/// <summary>
		/// Название уведомления.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Текст уведомления.
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Содержит URL-адрес изображения, которое будет загружено на устройство и отображено в уведомлении.<br/>
		/// JPEG, PNG, BMP полностью поддерживаются на всех платформах.<br/>
		/// Анимированные GIF-файлы и видео работают только на iOS. WebP и HEIF имеют разные уровни поддержки в зависимости от платформы и версии платформы.<br/>
		/// Android имеет ограничение на размер изображения в 1 МБ.<br/>
		/// Использование квоты и последствия/затраты на размещение изображений в Firebase Storage: https://firebase.google.com/pricing.
		/// </summary>
		public string Image { get; set; }
	}
}
