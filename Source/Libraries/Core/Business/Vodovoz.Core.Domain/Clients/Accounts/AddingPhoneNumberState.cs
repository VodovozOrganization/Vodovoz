using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients.Accounts
{
	/// <summary>
	/// Состояние добавления телефона
	/// </summary>
	public enum AddingPhoneNumberState
	{
		/// <summary>
		/// Ожидает добавления
		/// </summary>
		[Display(Name = "Ожидает добавления")]
		NeedAdd,
		/// <summary>
		/// Готово
		/// </summary>
		[Display(Name = "Готово")]
		Done
	}
}
