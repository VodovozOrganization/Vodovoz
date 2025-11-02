using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Contacts
{
	/// <summary>
	/// Назначение телефона
	/// </summary>
	public enum PhonePurpose
	{
		/// <summary>
		/// Стандартный
		/// </summary>
		[Display(Name = "Стандартный")]
		Default,
		/// <summary>
		/// Для чеков
		/// </summary>
		[Display(Name = "Для чеков")]
		ForReceipts
	}
}
