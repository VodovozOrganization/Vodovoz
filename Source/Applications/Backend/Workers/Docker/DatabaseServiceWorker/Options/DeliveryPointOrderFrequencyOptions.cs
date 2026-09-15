using System;

namespace DatabaseServiceWorker.Options
{
	/// <summary>
	/// Настройки фонового пересчёта частоты заказов точек доставки.
	/// </summary>
	internal sealed class DeliveryPointOrderFrequencyOptions
	{
		/// <summary>
		/// Пауза между полными проходами по точкам доставки.
		/// </summary>
		public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);

		/// <summary>
		/// Количество идентификаторов точек, загружаемых за один запрос.
		/// </summary>
		public int BatchSize { get; set; } = 100;

		/// <summary>
		/// Пауза между порциями для ограничения нагрузки на базу данных.
		/// </summary>
		public TimeSpan BatchDelay { get; set; } = TimeSpan.FromSeconds(1);
	}
}
