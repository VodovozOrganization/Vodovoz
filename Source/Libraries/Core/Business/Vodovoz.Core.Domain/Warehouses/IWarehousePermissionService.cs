using QS.DomainModel.UoW;
using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Warehouses
{
	public interface IWarehousePermissionService
	{
		IEnumerable<Warehouse> GetAvailableWarehousesForUser(IUnitOfWork unitOfWork, int userId);
		IEnumerable<Warehouse> GetAvailableWarehousesForUser(IUnitOfWork unitOfWork, int userId, WarehousePermissionsType warehousePermissionsType);
		Warehouse GetDefaultWarehouseForUser(IUnitOfWork unitOfWork, int userId);
	}
}
