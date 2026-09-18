using Vodovoz.EntityRepositories.Counterparties;

namespace DatabaseServiceWorker
{
	/// <summary>
	/// Результат успешно сохранённого расчёта и состояние исходных заказов.
	/// </summary>
	internal sealed class DeliveryPointOrderFrequencyCacheEntry
	{
		/// <summary>
		/// Количество выполненных заказов и идентификатор последнего выполненного заказа на момент расчёта.
		/// Отсутствие состояния означает промах кэша.
		/// </summary>
		public OrderFrequencyState OrderState { get; set; }

		/// <summary>
		/// Сохранённая частота заказов; null означает недостаточное количество заказов.
		/// </summary>
		public int? Frequency { get; set; }
	}
}
