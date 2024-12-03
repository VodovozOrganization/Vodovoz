using System.Collections.Generic;

namespace Pacs.Core.Messages.Events
{
	/// <summary>
	/// Событие глобальной смены доступности перерыва
	/// </summary>
	public class GlobalBreakAvailabilityEvent : EventBase
	{
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
		/// Причина недоступности малого перерыва
		/// </summary>
		public string ShortBreakDescription { get; set; } = "";

		/// <summary>
		/// Сравнение событий глобавльной доступности перерыва
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
			=> obj is GlobalBreakAvailabilityEvent availability
			&& LongBreakAvailable == availability.LongBreakAvailable
			&& LongBreakDescription == availability.LongBreakDescription
			&& ShortBreakAvailable == availability.ShortBreakAvailable
			&& ShortBreakDescription == availability.ShortBreakDescription;

		/// <summary>
		/// Получение хэш-кода
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			int hashCode = 622147706;
			hashCode = hashCode * -1521134295 + LongBreakAvailable.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LongBreakDescription);
			hashCode = hashCode * -1521134295 + ShortBreakAvailable.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ShortBreakDescription);
			return hashCode;
		}
	}
}
