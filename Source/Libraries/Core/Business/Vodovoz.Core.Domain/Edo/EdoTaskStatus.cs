using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum EdoTaskStatus
	{
		/// <summary>
		/// Задача которая еще не была взята в работу
		/// </summary>
		[Display(Name = "Новая")]
		New,

		/// <summary>
		/// Задача ожидающая решения внешних факторов
		/// </summary>
		[Display(Name = "Ожидание")]
		Waiting,

		/// <summary>
		/// Задача в работе
		/// </summary>
		[Display(Name = "В работе")]
		InProgress,

		/// <summary>
		/// Задача имеющая проблемы которые невозможно решить автоматически
		/// </summary>
		[Display(Name = "Проблема")]
		Problem,

		/// <summary>
		/// Задача завершена
		/// </summary>
		[Display(Name = "Завершена")]
		Completed,

		/// <summary>
		/// Отменяется
		/// </summary>
		[Display(Name = "Отменяется")]
		InCancellation,

		/// <summary>
		/// Отменена
		/// </summary>
		[Display(Name = "Отменена")]
		Cancelled
	}
}
