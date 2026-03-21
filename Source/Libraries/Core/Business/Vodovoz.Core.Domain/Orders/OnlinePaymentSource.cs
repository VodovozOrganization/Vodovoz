using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Источник оплаты с ИПЗ
	/// </summary>
	public enum OnlinePaymentSource
	{
		/// <summary>
		/// Сайт ВВ(Юкасса)
		/// </summary>
		[Display(Name = "Сайт ВВ")]
		FromVodovozWebSite,
		/// <summary>
		/// Сайт ВВ по QR(Авангард)
		/// </summary>
		[Display(Name = "Сайт ВВ по QR")]
		FromVodovozWebSiteByQr,
		/// <summary>
		/// Сайт ВВ Яндекс Сплит
		/// </summary>
		[Display(Name = "Сайт ВВ Я.Сплит")]
		FromVodovozWebSiteByYandexSplit,
		/// <summary>
		/// Мобильное приложение(CloudPayments)
		/// </summary>
		[Display(Name = "Мобильное приложение")]
		FromMobileApp,
		/// <summary>
		/// Мобильное приложение по QR(Авангард)
		/// </summary>
		[Display(Name = "Мобильное приложение по QR")]
		FromMobileAppByQr,
		/// <summary>
		/// МП Яндекс Сплит
		/// </summary>
		[Display(Name = "МП Я.Сплит")]
		FromMobileAppByYandexSplit,
		/// <summary>
		/// ИИ Бот по QR
		/// </summary>
		[Display(Name = "ИИ Бот по QR")]
		FromAiBotByQr
	}
}
