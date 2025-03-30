using QS.DomainModel.UoW;
using System.Collections.Generic;
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
	}
}
