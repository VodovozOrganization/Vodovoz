using System;

namespace TaxcomEdo.Contracts.Authorization
{
	/// <summary>
	/// Данные для авторизации по логину и паролю
	/// </summary>
	[Serializable]
	public class LoginDto
	{
		/// <summary>
		/// Логин
		/// </summary>
		public string Login { get; set; }
		/// <summary>
		/// Пароль
		/// </summary>
		public string Password { get; set; }
	}
}
