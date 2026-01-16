using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Contacts
{
	/// <summary>
	/// Предназначение электронной почты
	/// </summary>
	public enum EmailPurpose
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
		ForReceipts,
		/// <summary>
		/// Для счетов
		/// </summary>
		[Display(Name = "Для счетов")]
		ForBills,
		/// <summary>
		/// Рабочий
		/// </summary>
		[Display(Name = "Рабочий")]
		Work,
		/// <summary>
		/// Личный
		/// </summary>
		[Display(Name = "Личный")]
		Personal,
		/// <summary>
		/// К этой почте привязан аккаунт юр лица в ИПЗ
		/// </summary>
		[Display(Name = "Аккаунт юр лица в ИПЗ")]
		ExternalAccount
	}
}
