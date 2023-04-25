using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum PaymentType
	{
		[Display(Name = "Наличная", ShortName = "нал.")]
		Cash,
		[Display(Name = "Терминал (QR-код терминал)", ShortName = "терм.")]
		TerminalQR,
		[Display(Name = "МП водителя (QR-код)", ShortName = "МП вод.")]
		DriverApplicationQR,
		[Display(Name = "SMS (QR-код)", ShortName = "смс qr")]
		SmsQR,
		[Display(Name = "Оплачено онлайн", ShortName = "онлайн")]
		PaidOnline,
		[Display(Name = "Бартер", ShortName = "бар.")]
		Barter,
		[Display(Name = "Контрактная документация", ShortName = "контрактн.")]
		ContractDocumentation,
		[Display(Name = "Безналичная", ShortName = "б/н.")]
		Cashless,
	}
}
