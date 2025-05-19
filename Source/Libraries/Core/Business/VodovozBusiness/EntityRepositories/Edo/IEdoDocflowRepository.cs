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
		/// Получить данные по не обработанным отсканированным водителями кодам ЧЗ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список данные по необработанным кодам</returns>
		Task<IEnumerable<DriversScannedCodeDataNode>> GetAllNotProcessedDriversScannedCodesData(IUnitOfWork uow, CancellationToken cancellationToken);

		/// <summary>
		/// Получить запросы ЭДО по номеру заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список запросов на ЭДО</returns>
		Task<IEnumerable<OrderEdoRequest>> GetOrderEdoRequestsByOrderId(IUnitOfWork uow, int orderId, CancellationToken cancellationToken);
	}
}
