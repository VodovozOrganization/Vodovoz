using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Contacts
{
	public enum EmailPurpose
	{
		[Display(Name = "Стандартный")]
		Default,
		[Display(Name = "Для чеков")]
		ForReceipts,
		[Display(Name = "Для счетов")]
		ForBills,
		[Display(Name = "Рабочий")]
		Work,
		[Display(Name = "Личный")]
		Personal
	}
}
