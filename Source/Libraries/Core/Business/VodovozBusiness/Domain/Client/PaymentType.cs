using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	[Appellative(
		Nominative = "Вид оплаты",
		NominativePlural = "Виды оплаты")]
	public enum PaymentType
	{
		[Display(Name = "Наличная", ShortName = "нал.")]
		Cash,
		[Display(Name = "Терминал", ShortName = "терм.")]
		Terminal,
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
