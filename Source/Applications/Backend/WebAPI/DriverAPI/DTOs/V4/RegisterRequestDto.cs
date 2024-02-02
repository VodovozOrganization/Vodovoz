namespace DriverAPI.DTOs.V4
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
		
		/// <summary>
		/// Роль пользователя
		/// </summary>
		public string UserRole { get; set; }
	}
}
