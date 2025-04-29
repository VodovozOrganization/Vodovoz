namespace FastPaymentsApi.Contracts
{
	/// <summary>
	/// Типы источников запроса
	/// </summary>
	public enum RequestFromType
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
		FromMobileAppByQr
	}
}
