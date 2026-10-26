using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace VodovozBusiness.Services.Orders
{
	public interface IPendingAutoOrderSettingsOnlineOrderHandler
	{
		/// <summary>
		/// Перевод онлайн заказов ожидающих настройку автозаказа на ручную обработку по истечению лимита времени
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task TryMoveToManualProcessingPendingAutoOrderSettingsOnlineOrders(
			IUnitOfWork uow,
			CancellationToken cancellationToken
		);
	}
}
