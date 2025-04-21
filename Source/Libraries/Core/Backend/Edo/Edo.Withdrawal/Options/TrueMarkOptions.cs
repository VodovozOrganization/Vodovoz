using System;
using System.Collections.Generic;

namespace Edo.Withdrawal.Options
{
	/// <summary>
	/// Настройки Честного знака
	/// </summary>
	public class TrueMarkOptions
	{
		/// <summary>
		/// Интервал обработки документов ЧЗ
		/// </summary>
		public TimeSpan Interval { get; set; }

		/// <summary>
		/// Адрес Честного знака
		/// </summary>
		public string ExternalTrueMarkBaseUrl { get; set; }

		/// <summary>
		/// Сертификаты
		/// </summary>
		public IList<OrganizationCertificate> OrganizationCertificates { get; set; }
	}
}
