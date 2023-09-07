using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum CounterpartySubtype
	{
		[Display(Name = "Бартер")]
		Barter,
		[Display(Name = "Благотворительность")]
		Charity,
		[Display(Name = "Мероприятия")]
		Events
	}
}
