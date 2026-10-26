using System;

namespace Vodovoz.Core.Domain.Sale
{
	/// <summary>
	/// ВременнАя метка онлайн заказа
	/// </summary>
	public class OnlineOrderTimestamp
	{
		/// <summary>
		/// Дата и время начала действия
		/// </summary>
		public virtual DateTime StartTime { get; set; }

		/// <summary>
		/// Время действия в секундах
		/// </summary>
		public virtual int TimerSeconds { get; set; }

		/// <summary>
		/// Тип метки
		/// </summary>
		public virtual OnlineOrderTimestampType? OnlineOrderTimestampType { get; set; }

		public static OnlineOrderTimestamp Create(
			DateTime startTime,
			int timerSeconds,
			OnlineOrderTimestampType? onlineOrderTimestampType)
		{
			return new OnlineOrderTimestamp
			{
				StartTime = startTime,
				TimerSeconds = timerSeconds,
				OnlineOrderTimestampType = onlineOrderTimestampType
			};
		}
	}

	public enum OnlineOrderTimestampType
	{
		/// <summary>
		/// Время на оплату автозаказа
		/// </summary>
		AutoOrderPayTime,
		/// <summary>
		/// Время на оплату автозаказа, установленное оператором
		/// </summary>
		AutoOrderPayTimeByOperator,
		/// <summary>
		/// Время на настройку автозаказа
		/// </summary>
		AutoOrderSettingsTime,
		/// <summary>
		/// Время на настройку автозаказа, установленное оператором
		/// </summary>
		AutoOrderSettingsTimeByOperator
	}
}
