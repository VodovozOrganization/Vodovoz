using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Сервис кэширования типизированных идемпотентных запросов
	/// </summary>
	public interface IIdempotencyRequestCacheService<T>
		where T : class
	{
		/// <summary>
		/// Получение закэшированного ответа
		/// 
		/// Использует идентификатор запроса, использовать с идемпотентными запросами
		/// </summary>
		/// <typeparam name="T">Тип закэшированного ответа</typeparam>
		/// <param name="path">Путь ответа (uri)</param>
		/// <param name="requestId">Идентификатор запроса</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<ResponseInfo<T>> GetCachedResponse(string path, Guid requestId, CancellationToken cancellationToken);

		/// <summary>
		/// Запись ответа в кэш
		/// 
		/// Испольщует идентификатор запроса, использовать с идемпотентными запросами
		/// </summary>
		/// <typeparam name="T">Тип ответа</typeparam>
		/// <param name="path">Путь ответа (uri)</param>
		/// <param name="requestId">Идентификатор запроса</param>
		/// <param name="response">Ответ</param>
		/// <param name="expirationTime">Дельта времени, после которого удалится запись в кэше</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task CacheResponse(string path, Guid requestId, ResponseInfo<T> response, TimeSpan expirationTime, CancellationToken cancellationToken);
	}
}
