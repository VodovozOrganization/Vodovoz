using System;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;

namespace Vodovoz.Presentation.WebApi.Idempotency
{
	/// <summary>
	/// Чтение времени действия в мобильном приложении из заголовка <see cref="IdempotencyRequestHeadersNames.ActionTimeUtc"/> текущего запроса
	/// </summary>
	public interface IActionTimeUtcProvider
	{
		/// <summary>
		/// Получение времени действия из заголовка текущего запроса.
		/// Принимается только формат UTC <c>yyyy-MM-ddTHH:mm:ssZ</c> (например, <c>2026-08-12T13:34:08Z</c>),
		/// дробная часть секунд допускается. Остальные значения считаются некорректными
		/// </summary>
		/// <param name="actionTimeUtc">Время действия, <see cref="DateTimeKind.Utc"/></param>
		/// <param name="rawHeaderValue">Сырое значение заголовка (для логирования), пустое, если заголовка нет</param>
		/// <returns><see langword="true"/>, если заголовок есть и разобран</returns>
		bool TryGetActionTimeUtc(out DateTime actionTimeUtc, out string rawHeaderValue);
	}
}
