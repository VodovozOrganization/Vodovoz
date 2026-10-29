using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Статус синхронизации сделки с Битрикс24.
	/// </summary>
	public enum BitrixDealSynchronizationStatus
	{
		/// <summary>
		/// Требуется создать сделку в Битрикс24.
		/// </summary>
		[Display(Name = "Требуется создание сделки")]
		DealCreateRequired,

		/// <summary>
		/// Сделка создана и актуальна.
		/// </summary>
		[Display(Name = "Сделка актуальна")]
		DealActual,

		/// <summary>
		/// Требуется обновить созданную сделку в Битрикс24.
		/// </summary>
		[Display(Name = "Требуется обновление сделки")]
		DealUpdateRequired
	}
}
