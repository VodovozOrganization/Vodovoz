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
		[JsonPropertyName("login")]
		public string Login { get; set; }

		/// <summary>
		/// Пароль пользователя, захешированный функцией SHA-512 по стандарту SHS - FIPS 180-4, результат хеширования в нижнем регистре
		/// </summary>
		[JsonPropertyName("password")]
		public string Password { get; set; }
	}
}
