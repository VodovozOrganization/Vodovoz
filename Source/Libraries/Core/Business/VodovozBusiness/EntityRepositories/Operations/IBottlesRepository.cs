using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.EntityRepositories.Operations
{
	public interface IBottlesRepository
	{
		int GetBottlesDebtAtCounterparty(IUnitOfWork uow, int? counterpartyId, DateTime? before = null);
		[Obsolete("Используйте получение по Id")]
		int GetBottlesDebtAtCounterparty(IUnitOfWork uow, Counterparty counterparty, DateTime? before = null);
		int GetBottlesDebtAtDeliveryPoint(IUnitOfWork uow, int? deliveryPointId, DateTime? before = null);
		[Obsolete("Используйте получение по Id")]
		int GetBottlesDebtAtDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime? before = null);
		int GetBottlesDebtAtCounterpartyAndDeliveryPoint(IUnitOfWork uow, Counterparty counterparty, DeliveryPoint deliveryPoint, DateTime? before);
		int GetEmptyBottlesFromClientByOrder(IUnitOfWork uow, INomenclatureRepository nomenclatureRepository, Order order, int? excludeDocument = null);
		int GetBottleDebtBySelfDelivery(IUnitOfWork uow, Counterparty counterparty);

		/// <summary>
		/// Возвращает задолженность по бутылям для списка контрагентов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartiesIds">Список Id контрагентов</param>
		/// <returns>Задолженности по бутылям контрагентов</returns>
		IDictionary<int, BottlesBalanceQueryResult> GetCounterpartiesBottlesDebtData(IUnitOfWork uow, IEnumerable<int> counterpartiesIds);

		/// <summary>
		/// Возвращает задолженность по бутылям для списка точек доставки
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="deliveryPointIds">Список Id точек доставки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задолженности по бутылям в разрезе точек доставки</returns>
		Task<IDictionary<int, int>> GetBottlesDebtsByDeliveryPointsAsync(
			IUnitOfWork uow, IEnumerable<int> deliveryPointIds, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает задолженность по бутылям для списка контрагентов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartyIds">Список Id контрагентов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задолженности по бутылям в разрезе контрагентов</returns>
		Task<IDictionary<int, int>> GetBottlesDebtsByCounterpartiesAsync(
			IUnitOfWork uow, IEnumerable<int> counterpartyIds, CancellationToken cancellationToken);
	}
}
