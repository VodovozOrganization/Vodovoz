using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	public enum AddressTransferType
	{
		[Display(Name = "С допогрузкой на складе")]
		NeedToReload,
		[Display(Name = "С передачей товара от водителя")]
		FromHandToHand,
		[Display(Name = "Из свободных остатков получателя")]
		FromFreeBalance
	}
}
