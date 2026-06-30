namespace CustomerOrdersApi.Library.SiteOrdersImport.Config
{
	/// <summary>
	/// Настройки приёма выгрузки заказов и брошенных корзин с сайта (I-5840, контракт v1)
	/// </summary>
	public class SiteOrdersImportOptions
	{
		/// <summary>
		/// Путь к секции настроек в конфигурации
		/// </summary>
		public const string Path = "SiteOrdersImport";

		/// <summary>
		/// Секретное слово (SOURCE_SIGN), общее для нашей стороны и сайта.
		/// Используется при проверке токена запроса по формуле
		/// strtoupper(md5(strtoupper(md5(SOURCE_SIGN) . md5(date)))).
		/// </summary>
		public string SourceSign { get; set; }
	}
}
