using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum PaymentType
	{
		[Display(Name = "Наличная", ShortName = "нал.")]
		Cash,
		[Display(Name = "Терминал", ShortName = "терм.")]
		TerminalQR,
		[Display(Name = "Безналичная", ShortName = "б/н.")]
		DriverApplicationQR,
		[Display(Name = "Безналичная", ShortName = "б/н.")]
		SmsQR,
		[Display(Name = "По карте/SMS", ShortName = "карта")]
		PaidOnline,
		[Display(Name = "Бартер", ShortName = "бар.")]
		Barter,
		[Display(Name = "Контрактная документация", ShortName = "контрактн.")]
		ContractDocumentation,
		[Display(Name = "Безналичная", ShortName = "б/н.")]
		Cashless,
	}
}
