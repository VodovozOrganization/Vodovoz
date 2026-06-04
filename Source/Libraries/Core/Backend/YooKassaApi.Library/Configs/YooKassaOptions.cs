namespace YooKassaApi.Library.Configs
{
	/// <summary>
	/// Настройки для ЮKassa
	/// </summary>
	public class YooKassaOptions
	{
		/// <summary>
		/// Идентификатор магазина 
		/// </summary>
		public string ShopId { get; set; }

		/// <summary>
		/// Ключ API
		/// </summary>
		public string SecretKey { get; set; }

		/// <summary>
		/// Адрес доступа к API ЮKassa
		/// </summary>
		public string ApiUrl { get; set; } = "https://api.yookassa.ru/v3/";
	}
}
