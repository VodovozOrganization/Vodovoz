namespace TaxcomEdoApi.Config
{
	/// <summary>
	/// Данные для настройки апи по ЭДО(электронному документообороту)
	/// </summary>
	public sealed class TaxcomEdoApiOptions
	{
		public const string Position = "Api";
		
		/// <summary>
		/// Адрес сервиса
		/// </summary>
		public string BaseUrl { get; set; }
		/// <summary>
		/// Id организации от которой будет обмен по ЭДО
		/// </summary>
		public string IntegratorId { get; set; }
		/// <summary>
		/// Отпечаток сертификата
		/// </summary>
		public string CertificateThumbprint { get; set; }
		/// <summary>
		/// Id кабинета отправителя
		/// </summary>
		public string EdxClientId { get; set; }
	}
}
