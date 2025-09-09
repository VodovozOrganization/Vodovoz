namespace TaxcomEdoApi.Library.Config
{
	/// <summary>
	/// Эндпойнты для авторизации
	/// </summary>
	public sealed class AuthorizationUri
	{
		/// <summary>
		/// Эндпойнт авторизации по сертификату
		/// </summary>
		public string CertificateLoginUri { get; set; }
		/// <summary>
		/// Эндпойнт авторизации по логину/паролю
		/// </summary>
		public string LoginUri { get; set; }
	}
}
