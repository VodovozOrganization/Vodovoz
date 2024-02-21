using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V4
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
