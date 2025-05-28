using QS.DomainModel.UoW;
using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Warehouses
{
	public interface IWarehousePermissionService
	{
		IEnumerable<WarehouseEntity> GetAvailableWarehousesForUser(IUnitOfWork unitOfWork, int userId);
		IEnumerable<WarehouseEntity> GetAvailableWarehousesForUser(IUnitOfWork unitOfWork, int userId, WarehousePermissionsType warehousePermissionsType);
		WarehouseEntity GetDefaultWarehouseForUser(IUnitOfWork unitOfWork, int userId);
	}
}
