using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Состояние расхождения документооборота ЭДО
	/// </summary>
	public enum EdoDiscrepancyState
	{
		/// <summary>
		/// Расхождения нет
		/// </summary>
		[Display(Name = "Нет")]
		None,

		/// <summary>
		/// Расхождение обнаружено и требует разбора
		/// </summary>
		[Display(Name = "Требует разбора")]
		Active,

		/// <summary>
		/// Расхождение разобрано (переотправка или отмена заказа)
		/// </summary>
		[Display(Name = "Разобрано")]
		Resolved
	}
}
