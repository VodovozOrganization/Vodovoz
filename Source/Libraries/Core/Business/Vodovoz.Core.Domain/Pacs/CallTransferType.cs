using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public enum CallTransferType
	{
		[Display(Name = "Консультативный")]
		Consultative,

		[Display(Name = "Слепой")]
		Blind,

		[Display(Name = "Возврат слепого перевода")]
		ReturnBlind
	}
}
