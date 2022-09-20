using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.FastPayments
{
	public enum FastPaymentPayType
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "По карте")]
		ByCard,
		[Display(Name = "По Qr коду")]
		ByQrCode
	}
}
