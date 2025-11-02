using System;
using System.Collections.Generic;
using TrueMark.Contracts;

namespace TrueMarkWorker.Options
{
	public class TrueMarkWorkerOptions
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
		/// Адрес апи ЧЗ Водовоза
		/// </summary>
		public string InternalTrueMarkApiBaseUrl { get; set; }
		/// <summary>
		/// Токен авторизации в апи ЧЗ Водовоза
		/// </summary>
		public string AuthorizationToken { get; set; }
		/// <summary>
		/// Сертификаты
		/// </summary>
		public IList<OrganizationCertificate> OrganizationCertificates { get; set; }
	}
}
