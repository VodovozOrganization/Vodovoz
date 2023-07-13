using System.ComponentModel.DataAnnotations;

namespace DriverAPI.DTOs.V3
{
	public class EnablePushNotificationsRequestDto
	{
		/// <summary>
		/// Firebase - токен пользователя на который бьудут отправляться Push-уведомления
		/// </summary>
		[Required]
		public string Token { get; set; }
	}
}
