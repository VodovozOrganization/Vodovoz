using BitrixNotificationsSend.Contracts;
using BitrixNotificationsSend.Contracts.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace BitrixNotificationsSend.Client
{
	/// <summary>
	/// Клиент создания сделок в Битрикс24 пакетными запросами batch.json
	/// </summary>
	public interface IBitrixDealsClient
	{
		/// <summary>
		/// Пакетное создание сделок в Битрикс24 по клиентам, не сделавшим заказ к плановой дате
		/// Выполняется пакетным (batch) запросом с ограничением по количеству команд в запросе
		/// </summary>
		/// <param name="plannedOrders">
		/// Данные по плановым заказам,
		/// не более <see cref="BitrixApiLimits.MaxBatchCommandsCount"/> за один вызов
		/// </param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Результат отправки с ключами команд созданных сделок, ошибками по отдельным сделкам
		/// и данными об операционном бюджете Битрикс24
		/// </returns>
		Task<Result<BitrixBatchResult>> SendPlannedOrderDeals(
			IEnumerable<PlannedOrderDto> plannedOrders,
			CancellationToken cancellationToken);

		/// <summary>
		/// Пакетное создание сделок в Битрикс24 по клиентам, у которых был сервисный заказ
		/// </summary>
		/// <param name="lastServiceOrders">
		/// Данные по последним сервисным заказам,
		/// не более <see cref="BitrixApiLimits.MaxBatchCommandsCount"/> за один вызов
		/// </param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Результат отправки с ключами команд созданных сделок, ошибками по отдельным сделкам
		/// и данными об операционном бюджете Битрикс24
		/// </returns>
		Task<Result<BitrixBatchResult>> LastServiceOrderDeals(IEnumerable<LastServiceOrderDto> lastServiceOrders, CancellationToken cancellationToken);

		/// <summary>
		/// Пакетное создание сделок в Битрикс24 по недовозам.
		/// </summary>
		/// <param name="undeliveredOrders">
		/// Данные по недовозам,
		/// не более <see cref="BitrixApiLimits.MaxBatchCommandsCount"/> за один вызов.
		/// </param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Результат отправки с ключами команд созданных сделок, идентификаторами созданных сделок,
		/// ошибками по отдельным сделкам и данными об операционном бюджете Битрикс24.
		/// </returns>
		Task<Result<BitrixBatchResult>> SendUndeliveredOrderDeals(
			IEnumerable<UndeliveredOrderDto> undeliveredOrders,
			CancellationToken cancellationToken);

		/// <summary>
		/// Пакетное обновление сделок в Битрикс24 по недовозам.
		/// </summary>
		/// <param name="undeliveredOrders">
		/// Данные по недовозам,
		/// не более <see cref="BitrixApiLimits.MaxBatchCommandsCount"/> за один вызов.
		/// </param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Результат отправки с ключами команд обновленных сделок,
		/// ошибками по отдельным сделкам и данными об операционном бюджете Битрикс24.
		/// </returns>
		Task<Result<BitrixBatchResult>> UpdateUndeliveredOrderDeals(
			IEnumerable<UndeliveredOrderDto> undeliveredOrders,
			CancellationToken cancellationToken);

		/// <summary>
		/// Пакетное обновление сделок в Битрикс24 по планируемым заказам,
		/// по которым клиент создал заказ
		/// </summary>
		/// <param name="plannedOrderDealUpdates">
		/// Данные для обновления сделок,
		/// не более <see cref="BitrixApiLimits.MaxBatchCommandsCount"/> за один вызов
		/// </param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Результат отправки с ключами команд обновлённых сделок, ошибками по отдельным сделкам
		/// и данными об операционном бюджете Битрикс24
		/// </returns>
		Task<Result<BitrixBatchResult>> UpdatePlannedOrderDeals(
			IEnumerable<PlannedOrderDealUpdateDto> plannedOrderDealUpdates,
			CancellationToken cancellationToken);

		/// <summary>
		/// Пакетное чтение текущих стадий сделок из Битрикс24
		/// </summary>
		/// <param name="dealIds">
		/// Id сделок в Битрикс24,
		/// не более <see cref="BitrixApiLimits.MaxBatchCommandsCount"/> за один вызов
		/// </param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Стадии найденных сделок, id не найденных (удалённых) сделок
		/// и ошибки чтения по остальным сделкам
		/// </returns>
		Task<Result<BitrixDealsStagesResult>> GetDealsStages(
			IEnumerable<long> dealIds,
			CancellationToken cancellationToken);
	}
}
