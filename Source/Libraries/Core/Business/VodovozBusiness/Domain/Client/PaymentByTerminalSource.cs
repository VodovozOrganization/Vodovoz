using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum PaymentByTerminalSource
	{
		[Display(Name = "Картой", ShortName = "картой")]
		ByCard,
		[Display(Name = "QR-код", ShortName = "qr-код")]
		ByQR
	}
}
