using Microsoft.AspNetCore.Authentication;

namespace CustomerOrdersApi.Configs
{
	/// <summary>
	/// Настройки для аутентификации
	/// </summary>
	public class BasicAuthenticationOptions : AuthenticationSchemeOptions
	{
		/// <summary>
		/// Подпись МП
		/// </summary>
		public string MobileApp { get; set; }
		/// <summary>
		/// Подпись сайта
		/// </summary>
		public string VodovozWebSite { get; set; }
		/// <summary>
		/// Подпись Кулер сэйл
		/// </summary>
		public string KulerSaleWebSite { get; set; }
	}
}
