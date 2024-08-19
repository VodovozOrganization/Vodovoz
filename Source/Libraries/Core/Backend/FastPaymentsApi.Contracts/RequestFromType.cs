namespace FastPaymentsApi.Contracts
{
	/// <summary>
	/// Типы источников запроса
	/// </summary>
	public enum RequestFromType
	{
		/// <summary>
		/// ДВ или водительское приложение по Qr
		/// </summary>
		FromDesktopOrDriverAppByQr = 10,
		/// <summary>
		/// Сайт по Qr
		/// </summary>
		FromSiteByQr = 11,
		/// <summary>
		/// ДВ по карте
		/// </summary>
		FromDesktopByCard = 12,
		/// <summary>
		/// МП по Qr
		/// </summary>
		FromMobileAppByQr = 13
	}
}
