using System;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Постановка онлайн-заказа в очередь синхронизации с Mindbox
	/// </summary>
	public interface IMindboxOnlineOrderEnqueuer
	{
		/// <summary>
		/// Ставит онлайн-заказ в очередь синхронизации с Mindbox
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork, в котором сохраняется заказ</param>
		/// <param name="onlineOrder">Онлайн-заказ ERP</param>
		/// <param name="changeAt">Время последнего изменения в ERP</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача постановки в очередь</returns>
		Task EnqueueAsync(
			IUnitOfWork unitOfWork,
			OnlineOrder onlineOrder,
			DateTime changeAt,
			CancellationToken cancellationToken);

		/// <summary>
		/// Ставит в очередь отмену онлайн-заказа
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork, в котором загружен заказ</param>
		/// <param name="onlineOrderId">Идентификатор онлайн-заказа ERP</param>
		/// <param name="changeAt">Время отмены в ERP</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача постановки в очередь</returns>
		Task EnqueueCancelAsync(
			IUnitOfWork unitOfWork,
			int onlineOrderId,
			DateTime changeAt,
			CancellationToken cancellationToken);
	}
}
