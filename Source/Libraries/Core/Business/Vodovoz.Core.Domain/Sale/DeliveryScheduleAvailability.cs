namespace Vodovoz.Core.Domain.Sale
{
	/// <summary>
	/// Доступность интервала доставки
	/// </summary>
	public enum DeliveryScheduleAvailability
	{
		/// <summary>
		/// Доступен
		/// </summary>
		Available,
		/// <summary>
		/// Доступен, но не в дату доставки
		/// </summary>
		AvailableNotDeliveryDate,
		/// <summary>
		/// Не доступен. Интервал удален из правил доставки
		/// </summary>
		NotAvailable
	}
}
