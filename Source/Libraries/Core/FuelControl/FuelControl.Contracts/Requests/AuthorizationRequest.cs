using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Requests
{
	/// <summary>
	/// Запрос авторизации
	/// </summary>
	public class AuthorizationRequest
	{
		/// <summary>
		/// Логин пользователя
		/// </summary>
		[Required]
		public string Login { get; set; }

		/// <summary>
		/// Пароль пользователя
		/// </summary>
		[Required]
		public string Password { get; set; }

		/// <summary>
		/// Ключ API пользователя
		/// </summary>
		[Required]
		public string ApiKey { get; set; }
	}
}
