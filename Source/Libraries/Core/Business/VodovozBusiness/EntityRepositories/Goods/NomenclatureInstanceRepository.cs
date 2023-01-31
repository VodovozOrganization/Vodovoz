using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Remotion.Linq.EagerFetching;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.EntityRepositories.Goods
{
	public class NomenclatureInstanceRepository : INomenclatureInstanceRepository
	{
		public InventoryNomenclatureInstance GetInventoryNomenclatureInstance(IUnitOfWork uow, int id)
			=> uow.GetById<InventoryNomenclatureInstance>(id);

		public IList<InventoryNomenclatureInstance> GetInventoryInstancesByStorage(
			IUnitOfWork uow, OperationTypeByStorage operationTypeByStorage)
		{
			var query = GetQuery(operationTypeByStorage);
			
			switch(operationTypeByStorage)
			{
				case OperationTypeByStorage.Warehouse:
					query = uow.Session.QueryOver<InstanceGoodsAccountingOperation>()
						.Where(x => x.);
			}
			
		}

		public IQueryOver<InstanceGoodsAccountingOperation, InstanceGoodsAccountingOperation> GetQuery(
			OperationTypeByStorage operationTypeByStorage)
		{
			return QueryOver.Of<WarehouseInstanceGoodsAccountingOperation>();
		}
	}
}
