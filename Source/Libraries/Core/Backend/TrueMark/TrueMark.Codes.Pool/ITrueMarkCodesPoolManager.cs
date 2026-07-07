using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TrueMark.Codes.Pool
{
	public interface ITrueMarkCodesPoolManager
	{
		/// <summary>
		/// Удаляет коды из пула кодов
		/// </summary>
		/// <param name="codeIds">Идентификаторы кодов</param>
		void DeleteCodes(IEnumerable<int> codeIds);

		/// <summary>
		/// Асинхронно удаляет коды из пула кодов
		/// </summary>
		/// <param name="codeIds">Идентификаторы кодов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат операции</returns>
		Task DeleteCodesAsync(IEnumerable<int> codeIds, CancellationToken cancellationToken);

		/// <summary>
		/// Удаляет просроченные коды из пула кодов
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Количество удаленных кодов</returns>
		Task<int> DeleteExpiredCodesAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Получить общее количество кодов в пуле
		/// </summary>
		int GetTotalCount();

		/// <summary>
		/// Получить общее количество кодов в пуле асинхронно
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Общее количество кодов в пуле</returns>
		Task<int> GetTotalCountAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Получить общее количество кодов в пуле по GTIN
		/// </summary>
		/// <returns>Словарь с общим количеством кодов по GTIN</returns>
		IDictionary<string, long> GetTotalCountByGtin();

		/// <summary>
		/// Получить общее количество кодов в пуле по GTIN асинхронно
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Словарь с общим количеством кодов по GTIN</returns>
		Task<IDictionary<string, long>> GetTotalCountByGtinAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Продвигает коды, увеличивая их срок действия на указанное количество секунд
		/// </summary>
		/// <param name="codeIds">Идентификаторы кодов</param>
		/// <param name="extraSecond">Количество секунд для продления срока действия</param>
		void PromoteCodes(IEnumerable<int> codeIds, int extraSecond);

		/// <summary>
		/// Асинхронно продвигает коды, увеличивая их срок действия на указанное количество секунд
		/// </summary>
		/// <param name="codeIds">Идентификаторы кодов</param>
		/// <param name="extraSecond">Количество секунд для продления срока действия</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат операции</returns>
		Task PromoteCodesAsync(IEnumerable<int> codeIds, int extraSecond, CancellationToken cancellationToken);

		/// <summary>
		/// Выбирает коды из пула кодов
		/// </summary>
		/// <param name="count">Количество кодов для выбора</param>
		/// <param name="promoted">Флаг, указывающий, следует ли выбирать продвинутые коды</param>
		/// <returns>Перечень идентификаторов выбранных кодов</returns>
		IEnumerable<int> SelectCodes(int count, bool promoted);

		/// <summary>
		/// Выбирает коды из пула кодов асинхронно
		/// </summary>
		/// <param name="count">Количество кодов для выбора</param>
		/// <param name="promoted">Флаг, указывающий, следует ли выбирать продвинутые коды</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Перечень идентификаторов выбранных кодов</returns>
		Task<IEnumerable<int>> SelectCodesAsync(int count, bool promoted, CancellationToken cancellationToken);

		/// <summary>
		/// Выбирает коды для проверки из пула кодов
		/// </summary>
		/// <param name="count">Количество кодов для выбора</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Перечень идентификаторов выбранных кодов</returns>
		Task<IEnumerable<int>> SelectCodesForCheckAsync(int count, CancellationToken cancellationToken);

		/// <summary>
		/// Обновляет даты истечения срока действия кодов на основе словаря, где ключ - идентификатор кода, а значение - новая дата истечения срока действия
		/// </summary>
		/// <param name="codeExpirationMap">Словарь с обновленными датами истечения срока действия</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат операции</returns>
		Task UpdateCodesExpirationAsync(IDictionary<int, DateTime> codeExpirationMap, CancellationToken cancellationToken);
	}
}
