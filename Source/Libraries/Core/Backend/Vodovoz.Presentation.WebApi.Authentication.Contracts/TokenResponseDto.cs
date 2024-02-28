namespace Vodovoz.Presentation.WebApi.Authentication.Contracts
{
	/// <summary>
	/// Ответ сервера при успешной аутентификации
	/// </summary>
	public class TokenResponse
	{
		/// <summary>
		/// Access - токен (JWT)
		/// </summary>
		public string AccessToken { get; set; }

		/// <summary>
		/// Логин пользователя
		/// </summary>
		public string UserName { get; set; }
	}
}
