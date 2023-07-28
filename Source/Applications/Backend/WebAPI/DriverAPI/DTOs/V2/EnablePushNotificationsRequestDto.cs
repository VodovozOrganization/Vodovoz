using System.ComponentModel.DataAnnotations;

namespace DriverAPI.DTOs.V2
{
	/// <summary>
	/// Запрос на подписку на PUSH-сообщения
	/// </summary>
	public class EnablePushNotificationsRequestDto
	{
		/// <summary>
		/// Firebase - токен пользователя на который бьудут отправляться Push-уведомления
		/// </summary>
		[Required]
		public string Token { get; set; }
	}
}
