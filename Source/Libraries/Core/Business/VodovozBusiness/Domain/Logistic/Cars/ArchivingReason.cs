using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Cars
{
	public enum ArchivingReason
	{
		[Display(Name = "Продано")]
		Sales,
		[Display(Name = "Утиль")]
		Scrap,
		[Display(Name = "Наемная")]
		Hired
	}
}
