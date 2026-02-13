using Microsoft.AspNetCore.Authentication;

namespace CustomerAppsApi.Configs
{
	/// <summary>
	/// Настройки для аутентификации
	/// </summary>
	public class BasicAuthenticationOptions : AuthenticationSchemeOptions
	{
		/// <summary>
		/// Токен МП
		/// </summary>
		public string MobileAppToken { get; set; }
		/// <summary>
		/// Токен сайта ВВ
		/// </summary>
		public string VodovozWebSiteToken { get; set; }
	}
}
