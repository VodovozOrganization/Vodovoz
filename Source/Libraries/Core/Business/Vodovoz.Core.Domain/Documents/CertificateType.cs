using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Тип сертификата
	/// </summary>
	public enum CertificateType
	{
		[Display(Name = "Для ТМЦ")]
		Nomenclature
	}
}
