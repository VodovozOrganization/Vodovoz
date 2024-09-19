using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark
{
	public enum EdoDocumentType
	{
		[Display(Name = "УПД")]
		UPD,

		[Display(Name = "Счет")]
		Bill,
	}
}
