using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;

namespace Vodovoz.Core
{
	public class WarehousePermissionValidator : IWarehousePermissionValidator
	{
		private readonly IEnumerable<SubdivisionWarehousePermission> subdivisionWarehousePermissions;

		public WarehousePermissionValidator(IEnumerable<SubdivisionWarehousePermission> subdivisionWarehousePermissions)
		{
			this.subdivisionWarehousePermissions = subdivisionWarehousePermissions;
		}

		public IEnumerable<Warehouse> GetAllowedWarehouses(WarehousePermissionsType permissionType, Subdivision subdivision)
		{
			var warehouse = new List<SubdivisionWarehousePermission>();
			while (subdivision != null)
			{
				var permissions = subdivisionWarehousePermissions.Where(x =>
						x.WarehousePermissionType == permissionType
						&& x.Subdivision.Id == subdivision.Id);
				warehouse.AddRange(permissions);
				subdivision = subdivision.ParentSubdivision;
			}
			return warehouse.GroupBy(x => x.Warehouse.Id).Where(x=>x.First().PermissionValue == true).Select(x => x.First().Warehouse);
		}

		public bool Validate(WarehousePermissionsType warehousePermissionsType, Warehouse warehouse, User user)
		{
			if (warehouse is null) return false;
			if (user.IsAdmin) return true;
			return Validate(warehousePermissionsType, warehouse.Id);
		}

		public bool Validate(WarehousePermissionsType warehousePermissionsType, int warehouseId)
			=> subdivisionWarehousePermissions.SingleOrDefault(x =>
					x.Warehouse.Id == warehouseId && x.WarehousePermissionType == warehousePermissionsType).PermissionValue
				.Value;
	}
}
