using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Presentation.WebApi.Caching
{
	/// <summary>
	/// Сервис для кэширования типизированных ответов на запросы
	/// </summary>
	public interface IRequestCacheService<T>
		where T : class
	{
		/// <summary>
		/// Получение закэшированного ответа
		/// 
		/// Не использует идентификатор запроса, не использовать с идемпотентными запросами
		/// </summary>
		/// <typeparam name="T">Тип закэшированного ответа</typeparam>
		/// <param name="path">Путь ответа (uri)</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<ResponseInfo<T>> GetCachedResponse(string path, CancellationToken cancellationToken);

		/// <summary>
		/// Запись ответа в кэш
		/// 
		/// Не использует идентификатор запроса, не использовать с идемпотентными запросами
		/// </summary>
		/// <typeparam name="T">Тип ответа</typeparam>
		/// <param name="path">Путь ответа (uri)</param>
		/// <param name="response">Ответ</param>
		/// <param name="expirationTime">Дельта времени, после которого удалится запись в кэше</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task CacheResponse(string path, ResponseInfo<T> response, TimeSpan expirationTime, CancellationToken cancellationToken);
	}
}
