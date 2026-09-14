using Vodovoz.EntityRepositories.Counterparties;

namespace DatabaseServiceWorker
{
	/// <summary>
	/// Результат успешно сохранённого расчёта и состояние исходных заказов.
	/// </summary>
	internal sealed class DeliveryPointOrderFrequencyCacheEntry
	{
		/// <summary>
		/// Признаки заказов на момент расчёта.
		/// </summary>
		public OrderFrequencyState State { get; set; }

		/// <summary>
		/// Сохранённая частота заказов; null означает недостаточное количество заказов.
		/// </summary>
		public int? Frequency { get; set; }
	}
}
