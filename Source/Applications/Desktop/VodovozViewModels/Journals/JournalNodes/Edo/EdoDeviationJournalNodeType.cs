using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Edo
{
	/// <summary>
	/// Вид строки журнала отклонений документооборота ЭДО
	/// </summary>
	public enum EdoDeviationJournalNodeType
	{
		/// <summary>
		/// Узел заказа, под которым собраны его отклонения и проблемы
		/// </summary>
		[Display(Name = "Заказ")]
		Order,

		/// <summary>
		/// Отклонение документооборота
		/// </summary>
		[Display(Name = "Отклонения")]
		Deviation,

		/// <summary>
		/// Проблема документооборота
		/// </summary>
		[Display(Name = "Проблемы")]
		Problem,

		/// <summary>
		/// Задача в проблемном статусе, по которой нет действующей записи проблемы
		/// </summary>
		[Display(Name = "Неизвестная проблема")]
		UnknownProblem
	}
}
