using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Синхронизация состояния онлайн-заказа с Mindbox
	/// </summary>
	public interface IMindboxOnlineOrderSyncProcessor
	{
		/// <summary>
		/// Сравнивает текущее состояние заказа со снимком и отправляет изменения в Mindbox
		/// </summary>
		/// <param name="onlineOrderId">Идентификатор онлайн-заказа ERP</param>
		/// <param name="transactionId">Идентификатор транзакции для идемпотентности</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача синхронизации</returns>
		Task SyncAsync(int onlineOrderId, Guid transactionId, CancellationToken cancellationToken);
	}
}
