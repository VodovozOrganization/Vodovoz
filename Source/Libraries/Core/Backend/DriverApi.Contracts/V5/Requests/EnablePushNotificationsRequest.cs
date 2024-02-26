using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V5.Requests
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
