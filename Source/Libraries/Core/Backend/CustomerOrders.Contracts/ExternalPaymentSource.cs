using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts
{
	/// <summary>
	/// Источник оплаты с ИПЗ
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ExternalPaymentSource
	{
		/// <summary>
		/// Сайт ВВ(Юкасса)
		/// </summary>
		FromVodovozWebSite,
		/// <summary>
		/// Сайт ВВ по QR(Авангард)
		/// </summary>
		FromVodovozWebSiteByQr,
		/// <summary>
		/// Сайт ВВ Яндекс Сплит
		/// </summary>
		FromVodovozWebSiteByYandexSplit,
		/// <summary>
		/// Мобильное приложение(CloudPayments)
		/// </summary>
		FromMobileApp,
		/// <summary>
		/// Мобильное приложение по QR(Авангард)
		/// </summary>
		FromMobileAppByQr,
		/// <summary>
		/// МП Яндекс Сплит
		/// </summary>
		FromMobileAppByYandexSplit,
		/// <summary>
		/// ИИ Бот по QR
		/// </summary>
		FromAiBotByQr
	}
}
