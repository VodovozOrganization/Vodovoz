namespace DriverApi.Contracts.V5.Requests
{
	/// <summary>
	/// Запрос на регистрацию пользователя
	/// </summary>
	public class RegisterRequest
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
