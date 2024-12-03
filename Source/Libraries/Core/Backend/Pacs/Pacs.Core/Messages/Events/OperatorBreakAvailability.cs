using System;
using System.Collections.Generic;

namespace Pacs.Core.Messages.Events
{
	/// <summary>
	/// Событие смены доступности перерывов
	/// </summary>
	public class OperatorBreakAvailability
	{
		/// <summary>
		/// Идентификатор оператора
		/// </summary>
		public int OperatorId { get; set; }

		/// <summary>
		/// Доступность большого перерыва
		/// </summary>
		public bool LongBreakAvailable { get; set; } = true;

		/// <summary>
		/// Причина недоступности большого перерыва
		/// </summary>
		public string LongBreakDescription { get; set; } = "";

		/// <summary>
		/// Доступность малого перерыва
		/// </summary>
		public bool ShortBreakAvailable { get; set; } = true;

		/// <summary>
		/// Время когда будет доступен следующий малый перерыв
		/// </summary>
		public DateTime? ShortBreakSupposedlyAvailableAfter { get; set; } = null;

		/// <summary>
		/// Причина недоступности малого перерыва
		/// </summary>
		public string ShortBreakDescription { get; set; } = "";

		/// <summary>
		/// Сравнение событий доступности перерыва
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
			=> obj is OperatorBreakAvailability availability
			&& LongBreakAvailable == availability.LongBreakAvailable
			&& LongBreakDescription == availability.LongBreakDescription
			&& ShortBreakAvailable == availability.ShortBreakAvailable
			&& ShortBreakSupposedlyAvailableAfter == availability.ShortBreakSupposedlyAvailableAfter
			&& ShortBreakDescription == availability.ShortBreakDescription;

		/// <summary>
		/// Получение хэш-кода
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			int hashCode = -1461885315;
			hashCode = hashCode * -1521134295 + OperatorId.GetHashCode();
			hashCode = hashCode * -1521134295 + LongBreakAvailable.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LongBreakDescription);
			hashCode = hashCode * -1521134295 + ShortBreakAvailable.GetHashCode();
			hashCode = hashCode * -1521134295 + ShortBreakSupposedlyAvailableAfter.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ShortBreakDescription);
			return hashCode;
		}
	}
}
