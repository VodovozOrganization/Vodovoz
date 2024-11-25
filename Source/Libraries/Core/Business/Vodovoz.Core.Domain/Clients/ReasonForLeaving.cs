using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
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
