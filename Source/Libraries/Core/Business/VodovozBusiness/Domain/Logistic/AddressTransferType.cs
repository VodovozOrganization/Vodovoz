using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	public enum AddressTransferType
	{
		[Display(Name = "С допогрузкой на складе")]
		NeedToReload,
		[Display(Name = "С перегрузом от первого водителя")]
		FromDriverToDriver,
		[Display(Name = "Из текущих свободных остатков получателя")]
		FromFreeBalance
	}
}
