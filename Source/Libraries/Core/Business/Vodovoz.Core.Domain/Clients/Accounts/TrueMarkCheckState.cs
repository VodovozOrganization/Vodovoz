using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients.Accounts
{
	/// <summary>
	/// Состояние проверки регистрации в ЧЗ
	/// </summary>
	public enum TrueMarkCheckState
	{
		/// <summary>
		/// В процессе
		/// </summary>
		[Display(Name = "В процессе")]
		InProgress,
		/// <summary>
		/// Готово
		/// </summary>
		[Display(Name = "Готово")]
		Done,
		/// <summary>
		/// Ошибка
		/// </summary>
		[Display(Name = "Ошибка")]
		Error
	}
}
