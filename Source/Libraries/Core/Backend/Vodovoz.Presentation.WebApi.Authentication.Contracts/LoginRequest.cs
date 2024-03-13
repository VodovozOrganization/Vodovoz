using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Presentation.WebApi.Authentication.Contracts
{
	/// <summary>
	/// Учетные данные для авторизации
	/// </summary>
	public class LoginRequest
	{
		/// <summary>
		/// Логин пользователя
		/// </summary>
		[Required]
		public string Username { get; set; }

		/// <summary>
		/// Пароль пользователя
		/// </summary>
		[Required]
		public string Password { get; set; }
	}
}
