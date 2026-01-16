using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients.Accounts
{
	/// <summary>
	/// Состояние добавления ЭДО аккаунта
	/// </summary>
	public enum AddingEdoAccountState
	{
		/// <summary>
		/// Ожидает добавления
		/// </summary>
		[Display(Name = "Ожидает добавления")]
		NeedAdd,
		/// <summary>
		/// В процессе
		/// </summary>
		[Display(Name = "В процессе")]
		InProgress,
		/// <summary>
		/// Готово
		/// </summary>
		[Display(Name = "Готово")]
		Done
	}
}
