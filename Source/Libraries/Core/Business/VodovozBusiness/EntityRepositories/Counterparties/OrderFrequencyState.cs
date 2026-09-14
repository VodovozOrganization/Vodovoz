using System;

namespace Vodovoz.EntityRepositories.Counterparties
{
	/// <summary>
	/// Признаки изменения заказов, участвующих в расчёте частоты точки доставки.
	/// </summary>
	public class OrderFrequencyState
	{
		/// <summary>
		/// Количество заказов, соответствующих условиям расчёта.
		/// </summary>
		public long OrderCount { get; set; }

		/// <summary>
		/// Последняя версия изменения подходящих заказов; отсутствует для пустой выборки.
		/// </summary>
		public DateTime? LastOrderVersion { get; set; }
	}
}
