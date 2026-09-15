using System.Collections.Generic;
using Vodovoz.EntityRepositories.Counterparties;

namespace DatabaseServiceWorker
{
	/// <summary>
	/// Результат успешно сохранённого расчёта и состояние исходных заказов.
	/// </summary>
	internal sealed class DeliveryPointOrderFrequencyCacheEntry
	{
		/// <summary>
		/// Последние пять выполненных заказов на момент расчёта по убыванию даты доставки и идентификатора.
		/// </summary>
		public IList<OrderFrequencyState> State { get; set; }

		/// <summary>
		/// Сохранённая частота заказов; null означает недостаточное количество заказов.
		/// </summary>
		public int? Frequency { get; set; }
	}
}
