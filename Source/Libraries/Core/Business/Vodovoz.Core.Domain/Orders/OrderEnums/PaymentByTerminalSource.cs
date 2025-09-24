using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	[Appellative(
		Nominative = "Подтип оплаты по терминалу",
		NominativePlural = "Подтипы оплат по терминалу",
		GenitivePlural = "Подтипов оплат по терминалу")]
	public enum PaymentByTerminalSource
	{
		[Display(Name = "Картой", ShortName = "картой")]
		ByCard,
		[Display(Name = "QR-код", ShortName = "qr-код")]
		ByQR
	}
}
