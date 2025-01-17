﻿using System;
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
	}
}
