namespace DriverAPI.DTOs.V3
{
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
