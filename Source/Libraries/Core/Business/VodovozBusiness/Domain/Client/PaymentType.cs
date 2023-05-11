using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum PaymentType
	{
		[Display(Name = "Наличная", ShortName = "нал.")]
		cash,
		[Display(Name = "По карте/SMS", ShortName = "карта")]
		ByCard,
		[Display(Name = "Терминал", ShortName = "терм.")]
		Terminal,
		[Display(Name = "Бартер", ShortName = "бар.")]
		barter,
		[Display(Name = "Контрактная документация", ShortName = "контрактн.")]
		ContractDoc,
		[Display(Name = "Безналичная", ShortName = "б/н.")]
		cashless,
	}
}
