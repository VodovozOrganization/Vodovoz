using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Тип заявки ЭДО
	/// </summary>
	public enum EdoRequestType
	{
		/// <summary>
		/// Первичный
		/// </summary>
		[Display(Name = "Первичный")]
		Primary,

		/// <summary>
		/// Ручная отправка
		/// </summary>
		[Display(Name = "Ручная отправка")]
		Manual,

		/// <summary>
		/// Вывод из оборота
		/// </summary>
		[Display(Name = "Вывод из оборота")]
		Withdrawal
	}
}
