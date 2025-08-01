namespace CustomerOrdersApi.Library.Config
{
	/// <summary>
	/// Подписи
	/// </summary>
	public class SignatureOptions
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
		/// Подпись Кулер сэйл
		/// </summary>
		public string KulerSaleWebSite { get; set; }
	}
}
