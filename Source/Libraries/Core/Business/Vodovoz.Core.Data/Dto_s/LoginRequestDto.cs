using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Data.Dto_s
{
	/// <summary>
	/// Учетные данные для авторизации
	/// </summary>
	public class LoginRequestDto
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
