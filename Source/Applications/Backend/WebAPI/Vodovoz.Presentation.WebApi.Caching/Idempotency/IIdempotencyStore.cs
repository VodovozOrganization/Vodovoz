using System;
using System.Threading.Tasks;

namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Хранилище записей идемпотентных запросов
	/// </summary>
	public interface IIdempotencyStore
	{
		/// <summary>
		/// Атомарно ставит метку «в обработке», если записи с таким ключом ещё нет
		/// </summary>
		/// <param name="key">Ключ записи</param>
		/// <param name="markerRecord">Метка «в обработке»</param>
		/// <param name="markerLifetime">Срок жизни метки</param>
		/// <returns>Результат попытки</returns>
		Task<IdempotencyBeginResult> TryBeginAsync(string key, IdempotencyRecord markerRecord, TimeSpan markerLifetime);

		/// <summary>
		/// Чтение записи. Нечитаемая запись (ошибка десериализации, неизвестная версия формата)
		/// возвращается как <see cref="IdempotencyReadStatus.Unavailable"/>
		/// </summary>
		/// <param name="key">Ключ записи</param>
		/// <returns>Результат чтения</returns>
		Task<IdempotencyReadResult> GetAsync(string key);

		/// <summary>
		/// Сохранение ответа поверх метки «в обработке»
		/// </summary>
		/// <param name="key">Ключ записи</param>
		/// <param name="record">Запись с сохранённым ответом</param>
		/// <param name="lifetime">Срок хранения от момента сохранения</param>
		/// <returns><see langword="true"/>, если запись сохранена</returns>
		Task<bool> SaveCompletedAsync(string key, IdempotencyRecord record, TimeSpan lifetime);

		/// <summary>
		/// Удаление записи (снятие метки «в обработке»)
		/// </summary>
		/// <param name="key">Ключ записи</param>
		/// <returns><see langword="true"/>, если команда выполнена (в том числе когда записи уже не было)</returns>
		Task<bool> RemoveAsync(string key);
	}
}
