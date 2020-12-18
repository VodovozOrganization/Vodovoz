using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Parameters;

namespace Vodovoz.Repository.Operations
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Operations")]
	public static class BottlesRepository
	{
		[Obsolete]
		public static int GetBottlesAtCounterparty(IUnitOfWork uow, Counterparty counterparty, DateTime? before = null)
		{
			return new EntityRepositories.Operations.BottlesRepository().GetBottlesAtCounterparty(
				uow,
				counterparty,
				before
			);
		}

		[Obsolete]
		public static int GetBottlesAtDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime? before = null)
		{
			return new EntityRepositories.Operations.BottlesRepository().GetBottlesAtDeliveryPoint(
				uow,
				deliveryPoint,
				before
			);
		}

		[Obsolete]
		public static int GetBottlesAtCouterpartyAndDeliveryPoint(IUnitOfWork uow, Counterparty counterparty, DeliveryPoint deliveryPoint, DateTime? before = null)
		{
			return new EntityRepositories.Operations.BottlesRepository().GetBottlesAtCouterpartyAndDeliveryPoint(
				uow,
				counterparty,
				deliveryPoint,
				before
			);
		}

		[Obsolete]
		public static int GetEmptyBottlesFromClientByOrder(IUnitOfWork uow, Order order, int? excludeDocument = null)
		{
			return new EntityRepositories.Operations.BottlesRepository().GetEmptyBottlesFromClientByOrder(
				uow,
				new EntityRepositories.Goods.NomenclatureRepository(new NomenclatureParametersProvider()),
				order,
				excludeDocument
			);
		}
	}
}