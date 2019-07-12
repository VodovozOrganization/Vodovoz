using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Repository.Store
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Store")]
	public static class SelfDeliveryRepository
	{
		[Obsolete]
		public static Dictionary<int,decimal> NomenclatureUnloaded(IUnitOfWork uow, Order order, SelfDeliveryDocument excludeDoc)
		{
			return new EntityRepositories.Store.SelfDeliveryRepository().NomenclatureUnloaded(uow, order, excludeDoc);
		}

		[Obsolete]
		public static Dictionary<int, decimal> OrderNomenclaturesLoaded(IUnitOfWork uow, Order order)
		{
			return new EntityRepositories.Store.SelfDeliveryRepository().OrderNomenclaturesLoaded(uow, order);
		}

		[Obsolete]
		public static Dictionary<int, decimal> OrderNomenclaturesUnloaded(IUnitOfWork uow, Order order, SelfDeliveryDocument notSavedDoc = null)
		{
			return new EntityRepositories.Store.SelfDeliveryRepository().OrderNomenclaturesUnloaded(uow, order, notSavedDoc);
		}
	}
}