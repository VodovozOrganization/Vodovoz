namespace Vodovoz.Core.Data.Dto_s
{
	/// <summary>
	/// Ответ сервера при успешной аутентификации
	/// </summary>
	public class TokenResponseDto
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
