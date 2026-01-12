using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients.Accounts
{
	/// <summary>
	/// Состояние добавления причины покупки воды
	/// </summary>
	public enum AddingReasonForLeavingState
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
