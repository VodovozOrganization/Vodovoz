namespace TrueMark.Contracts
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

		/// <summary>
		/// Путь на сервере
		/// </summary>
		public string CertPath { get; set; }

		/// <summary>
		/// Пароль
		/// </summary>
		public string CertPwd { get; set; }
	}
}
