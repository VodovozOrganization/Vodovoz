using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Contacts
{
	public enum PhonePurpose
	{
		[Display(Name = "Стандартный")]
		Default,
		[Display(Name = "Для чеков")]
		ForReceipts
	}
}
