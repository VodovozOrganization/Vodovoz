using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum PaymentByTerminalSource
	{
		[Display(Name = "По карте", ShortName = "карта")]
		ByCard,
		[Display(Name = "По QR-коду", ShortName = "qr-код")]
		ByQR
	}
}
