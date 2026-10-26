using System;

namespace CustomerOrders.Contracts.V8.Common
{
	/// <summary>
	/// Данные таймера по доп настройкам онлайн заказа
	/// </summary>
	public class OnlineOrderTimerSetupDto
	{
		/// <summary>
		/// Стартовая метка. От какого значения нужно вести отсчет
		/// </summary>
		public DateTimeOffset Timestamp { get; set; }

		/// <summary>
		/// Время, выделенное на действие в секундах
		/// </summary>
		public int TimeInSeconds { get; set; }

		public static OnlineOrderTimerSetupDto Create(DateTimeOffset startingMark, int timeInSeconds) =>
			new OnlineOrderTimerSetupDto
			{
				Timestamp = startingMark,
				TimeInSeconds = timeInSeconds
			};
	}
}
