using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	[Appellative(
		Nominative = "Подтип оплаты по терминалу",
		NominativePlural = "Подтипы оплаты по терминалу")]
	public enum PaymentByTerminalSource
	{
		[Display(Name = "Картой", ShortName = "картой")]
		ByCard,
		[Display(Name = "QR-код", ShortName = "qr-код")]
		ByQR
	}
}
