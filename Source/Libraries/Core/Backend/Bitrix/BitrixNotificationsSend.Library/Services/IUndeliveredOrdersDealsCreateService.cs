using System;
using System.Threading;
using System.Threading.Tasks;

namespace BitrixNotificationsSend.Library.Services
{
	public interface IUndeliveredOrdersDealsCreateService
	{
		/// <summary>
		/// Собирает новые и измененные недовозы в локальное состояние синхронизации с Битрикс24.
		/// </summary>
		/// <param name="minLastEditedTime">Минимальное время изменения недовоза для попадания в сбор.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Количество созданных или обновленных записей синхронизации.</returns>
		Task<int> CollectUndeliveredOrders(DateTime minLastEditedTime, CancellationToken cancellationToken);

		/// <summary>
		/// Отправляет в Битрикс24 запросы на создание сделок по недовозам со статусом "Требуется создание сделки".
		/// После успешного создания сохраняет идентификатор сделки Битрикс24.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены.</param>
		Task SendNotCreatedDeals(CancellationToken cancellationToken);

		/// <summary>
		/// Отправляет в Битрикс24 запросы на обновление созданных сделок по измененным недовозам.
		/// После успешного обновления помечает сделку актуальной.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены.</param>
		Task SendNotActualDeals(CancellationToken cancellationToken);
	}
}
