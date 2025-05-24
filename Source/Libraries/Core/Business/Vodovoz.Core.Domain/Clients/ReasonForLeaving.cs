using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Причина выбытия из документооборота
	/// </summary>
	public enum ReasonForLeaving
	{
		/// <summary>
		/// Неизвестно
		/// </summary>
		[Display(Name = "Неизвестно")]
		Unknown,
		/// <summary>
		/// Для собственных нужд
		/// </summary>
		[Display(Name = "Для собственных нужд")]
		ForOwnNeeds,
		/// <summary>
		/// Перепродажа
		/// </summary>
		[Display(Name = "Перепродажа")]
		Resale,
		/// <summary>
		/// Госзакупки
		/// </summary>
		[Display(Name = "Госзакупки")]
		Tender,
		/// <summary>
		/// Иная
		/// </summary>
		[Display(Name = "Иная")]
		Other
	}
}
