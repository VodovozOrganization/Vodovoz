using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum ReasonForLeaving
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "Для собственных нужд")]
		ForOwnNeeds,
		[Display(Name = "Перепродажа")]
		Resale,
		[Display(Name = "Иная")]
		Other
	}
}
