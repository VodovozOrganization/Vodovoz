namespace DriverAPI.DTOs.V2
{
	/// <summary>
	/// Запрос на регистрацию пользователя
	/// </summary>
	public class RegisterRequestDto
	{
		/// <summary>
		/// Логин пользователя
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Пароль пользователя
		/// </summary>
		public string Password { get; set; }
	}
}
