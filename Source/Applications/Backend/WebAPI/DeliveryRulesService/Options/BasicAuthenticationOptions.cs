using Microsoft.AspNetCore.Authentication;

namespace DeliveryRulesService.Options
{
	/// <summary>
	/// Настройки для аутентификации
	/// </summary>
	public class BasicAuthenticationOptions : AuthenticationSchemeOptions
	{
		public const string Path = "Signatures";
		
		/// <summary>
		/// Подпись МП
		/// </summary>
		public string MobileApp { get; set; }
		/// <summary>
		/// Подпись сайта
		/// </summary>
		public string VodovozWebSite { get; set; }
		/// <summary>
		/// Подпись ИИ бота
		/// </summary>
		public string AiBot { get; set; }
		/// <summary>
		/// Подпись Кулер сэйл
		/// </summary>
		public string KulerSaleWebSite { get; set; }
	}
}
