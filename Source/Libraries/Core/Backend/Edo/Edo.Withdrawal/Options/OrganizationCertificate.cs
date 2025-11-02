namespace Edo.Withdrawal.Options
{
	/// <summary>
	/// Сертификат организации
	/// </summary>
	public class OrganizationCertificate
	{
		/// <summary>
		/// Идентификатор участника
		/// </summary>
		public string EdxClientId { get; set; }

		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }

		/// <summary>
		/// Отпечаток сертификата
		/// </summary>
		public string CertificateThumbPrint { get; set; }
	}
}
