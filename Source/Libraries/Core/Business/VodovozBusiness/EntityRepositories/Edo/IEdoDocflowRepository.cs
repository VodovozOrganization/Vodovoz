using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using VodovozBusiness.Nodes;

namespace VodovozBusiness.EntityRepositories.Edo
{
	public interface IEdoDocflowRepository
	{
		/// <summary>
		/// Получить данные документооборота по номеру заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>Список данных документооборота</returns>
		IList<EdoDockflowData> GetEdoDocflowDataByOrderId(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получить данные документооборота по номеру клиента
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="clientId">Номер клиента</param>
		/// <returns>Список данных документооборота</returns>
		IList<EdoDockflowData> GetEdoDocflowDataByClientId(IUnitOfWork uow, int clientId);

		/// <summary>
		/// Получить запросы ЭДО по номеру заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список запросов на ЭДО</returns>
		Task<IEnumerable<OrderEdoRequest>> GetOrderEdoRequestsByOrderId(IUnitOfWork uow, int orderId, CancellationToken cancellationToken);

		/// <summary>
		/// Возварщает данные по не обработанным отсканированным водителями кодам ЧЗ по идентификатору адреса МЛ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="routeListItemId">Идентификатор адреса МЛ</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список данных по необработанным кодам</returns>
		Task<IEnumerable<DriversScannedCodeDataNode>> GetNotProcessedDriversScannedCodesDataByRouteListItemId(IUnitOfWork uow, int routeListItemId, CancellationToken cancellationToken);

		/// <summary>
		/// Возварщает идентификаторы адресов МЛ, по которым есть не обработанные отсканированные водителями коды ЧЗ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список адресов МЛ</returns>
		Task<IEnumerable<int>> GetNotProcessedDriversScannedCodesRouteListAddressIds(IUnitOfWork uow, CancellationToken cancellationToken);
	}
}
