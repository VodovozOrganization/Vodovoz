using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.EntityRepositories.Operations
{
	public interface IBottlesRepository
	{
		int GetBottlesDebtAtCounterparty(IUnitOfWork uow, int counterpartyId, DateTime? before = null);
		int GetBottlesDebtAtDeliveryPoint(IUnitOfWork uow, int deliveryPointId, DateTime? before = null);
		int GetBottlesDebtAtCounterpartyAndDeliveryPoint(IUnitOfWork uow, int counterpartyId, int deliveryPointId, DateTime? before);
		int GetEmptyBottlesFromClientByOrder(IUnitOfWork uow, INomenclatureRepository nomenclatureRepository, Order order, int? excludeDocument);
		int GetBottleDebtBySelfDelivery(IUnitOfWork uow, int counterpartyId);
	}
}
