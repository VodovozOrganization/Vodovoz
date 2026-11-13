using System;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Приостанавливает вызовы расчёта бонусов при серии временных ошибок Mindbox
	/// </summary>
	public interface IMindboxBonusCircuitBreaker
	{
		/// <summary>
		/// Дата окончания текущей паузы
		/// </summary>
		DateTime? PausedUntil { get; }

		/// <summary>
		/// Проверяет, находится ли расчёт бонусов на паузе
		/// </summary>
		/// <param name="now">Текущее время</param>
		/// <returns><c>true</c>, если вызовы Mindbox приостановлены; иначе <c>false</c></returns>
		bool IsPaused(DateTime now);

		/// <summary>
		/// Пытается начать вызов Mindbox. После паузы пропускает только один пробный вызов
		/// </summary>
		/// <param name="now">Текущее время</param>
		/// <returns><c>true</c>, если вызов разрешён; <c>false</c>, если действует пауза
		/// или пробный вызов уже выполняется</returns>
		bool TryBeginCall(DateTime now);

		/// <summary>
		/// Учитывает успешный вызов и сбрасывает паузу и счётчик ошибок
		/// </summary>
		void RegisterSuccess();

		/// <summary>
		/// Учитывает ошибку вызова и при достижении порога включает паузу
		/// </summary>
		/// <param name="now">Текущее время</param>
		/// <param name="pauseEligible">Признак того, что ошибка учитывается при включении паузы
		/// (5xx, 429, таймаут или отсутствие соединения)</param>
		/// <param name="reason">Причина ошибки для диагностики</param>
		void RegisterFailure(DateTime now, bool pauseEligible, string reason);
	}
}
