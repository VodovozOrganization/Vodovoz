using System.ComponentModel.DataAnnotations;

namespace LogisticsEventsApi.Contracts
{
	/// <summary>
	/// Запрос на подписку на PUSH-сообщения
	/// </summary>
	public class EnablePushNotificationsRequest
	{
		/// <summary>
		/// Firebase - токен пользователя на который бьудут отправляться Push-уведомления
		/// </summary>
		[Required]
		public string Token { get; set; }
	}
}
