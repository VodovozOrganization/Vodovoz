using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Permissions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Permissions.Warehouse;
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

		public IEnumerable<Warehouse> GetAllowedWarehouses(WarehousePermissions permission, Subdivision subdivision)
		{
			var warehouse = new List<SubdivisionWarehousePermission>();
			while (subdivision != null)
			{
				var permissions = subdivisionWarehousePermissions.Where(x =>
						x.WarehousePermissionType == permission
						&& x.Subdivision.Id == subdivision.Id);
				warehouse.AddRange(permissions);
				subdivision = subdivision.ParentSubdivision;
			}
			return warehouse.GroupBy(x => x.Warehouse.Id).Where(x=>x.First().ValuePermission == true).Select(x => x.First().Warehouse);
		}

		public bool Validate(WarehousePermissions warehousePermissions, Warehouse warehouse, User user)
		{
			if (warehouse is null) return false;
			if (user.IsAdmin) return true;
			return Validate(warehousePermissions, warehouse.Id);
		}

		public bool Validate(WarehousePermissions warehousePermissions, int warehouseId)
			=> subdivisionWarehousePermissions.SingleOrDefault(x =>
					x.Warehouse.Id == warehouseId && x.WarehousePermissionType == warehousePermissions).ValuePermission
				.Value;
	}
}
