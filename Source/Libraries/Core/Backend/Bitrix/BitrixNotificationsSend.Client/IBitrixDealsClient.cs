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
	}
}
