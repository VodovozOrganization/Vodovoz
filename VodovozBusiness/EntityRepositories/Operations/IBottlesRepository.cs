using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.EntityRepositories.Operations
{
	public interface IBottlesRepository
	{
		int GetBottlesAtCounterparty(IUnitOfWork UoW, Counterparty counterparty, DateTime? before = null);
		int GetBottlesAtDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, DateTime? before = null);
		int GetBottlesAtCouterpartyAndDeliveryPoint(IUnitOfWork UoW, Counterparty counterparty, DeliveryPoint deliveryPoint, DateTime? before);
		int GetEmptyBottlesFromClientByOrder(IUnitOfWork uow, INomenclatureRepository nomenclatureRepository, Order order, int? excludeDocument);
	}
}