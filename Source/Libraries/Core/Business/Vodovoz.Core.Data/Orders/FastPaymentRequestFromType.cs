namespace Vodovoz.Core.Data.Orders
{
	/// <summary>
	/// Тип источника запроса быстрого платежа или по карте
	/// </summary>
	public enum FastPaymentRequestFromType
	{
		/// <summary>
		/// ДВ по Qr
		/// </summary>
		FromDesktopByQr,
		/// <summary>
		/// Водительское приложение по Qr
		/// </summary>
		FromDriverAppByQr,
		/// <summary>
		/// Сайт по Qr
		/// </summary>
		FromSiteByQr,
		/// <summary>
		/// ДВ по карте
		/// </summary>
		FromDesktopByCard,
		/// <summary>
		/// МП по Qr
		/// </summary>
		FromMobileAppByQr,
		/// <summary>
		/// ИИ Бот по Qr
		/// </summary>
		FromAiBotByQr
	}
}
