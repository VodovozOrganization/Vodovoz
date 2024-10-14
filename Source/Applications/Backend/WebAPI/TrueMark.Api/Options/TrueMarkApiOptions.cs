using System.Collections.Generic;
using TrueMark.Contracts;

namespace TrueMark.Api.Options;

public class TrueMarkApiOptions
{
	/// <summary>
	/// Адрес Честного знака
	/// </summary>
	public string ExternalTrueMarkBaseUrl { get; set; }
	/// <summary>
	/// Ключ для доступа к TrueMarkApi
	/// </summary>
	public string InternalSecurityKey { get; set; }
	/// <summary>
	/// Сертификаты
	/// </summary>
	public IList<OrganizationCertificate> OrganizationCertificates { get; set; }
}
