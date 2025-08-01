using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Tools.Store
{
	public interface IStoreDocumentHelper
	{
		bool CanEditDocument(WarehousePermissionsType edit, params Warehouse[] warehouses);
		bool CheckAllPermissions(bool isNew, WarehousePermissionsType edit, params Warehouse[] warehouses);
		bool CheckCreateDocument(WarehousePermissionsType edit, params Warehouse[] warehouses);
		bool CheckViewWarehouse(WarehousePermissionsType edit, params Warehouse[] warehouses);
		Warehouse GetDefaultWarehouse(IUnitOfWork uow, WarehousePermissionsType edit);
		QueryOver<Warehouse> GetRestrictedWarehouseQuery();
		QueryOver<Warehouse> GetRestrictedWarehouseQuery(params WarehousePermissionsType[] permissions);
		IEnumerable<int> GetRestrictedWarehousesIds(IUnitOfWork uow, params WarehousePermissionsType[] permissions);
		IList<Warehouse> GetRestrictedWarehousesList(IUnitOfWork uow, params WarehousePermissionsType[] permissions);
	}
}