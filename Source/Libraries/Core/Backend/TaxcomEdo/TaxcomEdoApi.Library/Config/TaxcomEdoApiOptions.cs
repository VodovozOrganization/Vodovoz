namespace TaxcomEdoApi.Library.Config
{
	/// <summary>
	/// Данные для настройки апи по ЭДО(электронному документообороту) от Такскома
	/// </summary>
	public sealed class TaxcomEdoApiOptions
	{
		public const string Path = "ApiOptions";
		
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
