using BitrixNotificationsSend.Contracts.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace BitrixNotificationsSend.Client
{
	/// <summary>
	/// Отправляет уведомления по плановым заказам в Битрикс24
	/// </summary>
	public interface IPlannedOrdersNotificationsSendClient
	{
		/// <summary>
		/// Создание сделки в Битрикс24 по клиенту, не сделавшему заказ к плановой дате
		/// </summary>
		/// <param name="plannedOrders">Данные по плановому заказу</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат отправки</returns>
		Task<Result> CreatePlannedOrderDeal(PlannedOrderDto plannedOrder, CancellationToken cancellationToken);
	}
}
