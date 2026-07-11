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
		/// Отправка уведомления в Битрикс24 по клиентам, не сделавшим заказ к плановой дате
		/// </summary>
		/// <param name="plannedOrders">Список с данными по плановым заказам</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат отправки</returns>
		Task<Result> SendPlannedOrdersNotification(IEnumerable<PlannedOrderDto> plannedOrders, CancellationToken cancellationToken);
	}
}
