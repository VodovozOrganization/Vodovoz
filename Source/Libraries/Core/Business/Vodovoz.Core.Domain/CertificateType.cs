using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain
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
