using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum EdoDocumentType
	{
		[Display(Name = "УПД")]
		UPD,

		[Display(Name = "Счет")]
		Bill,
	}
}
