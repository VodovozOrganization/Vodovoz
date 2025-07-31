namespace CustomerOrdersApi.Library.Config
{
	/// <summary>
	/// Данные по подписям ИПЗ
	/// </summary>
	public class SignatureOptions
	{
		public const string Position = "Signatures";

		/// <summary>
		/// Подпись сайта ВВ
		/// </summary>
		public string VodovozWebSite { get; set; }
		/// <summary>
		/// Подпись МП
		/// </summary>
		public string MobileApp { get; set; }
		/// <summary>
		/// Подпись сайта Кулер Сэйл
		/// </summary>
		public string KulerSaleWebSite { get; set; }
	}
}
