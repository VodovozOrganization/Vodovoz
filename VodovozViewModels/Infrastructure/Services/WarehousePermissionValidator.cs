using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;

namespace Vodovoz.Core
{
	public class WarehousePermissionValidator : IWarehousePermissionValidator
	{
		private readonly IEnumerable<SubdivisionWarehousePermission> _subdivisionWarehousePermissions;

		public WarehousePermissionValidator(IEnumerable<SubdivisionWarehousePermission> subdivisionWarehousePermissions)
		{
			_subdivisionWarehousePermissions = subdivisionWarehousePermissions;
		}

		public IEnumerable<Warehouse> GetAllowedWarehouses(WarehousePermissionsType permissionType, Employee employee)
		{
			// var userId = ServicesConfig.UserService.CurrentUserId;
			// using(var uow = UnitOfWorkFactory.CreateForRoot<User>(userId))
			// {
			// 	var employee = new EmployeeRepository().GetEmployeeForCurrentUser(uow);
			// 	var subdivision = employee.Subdivision;
			// 	permissions = new List<WarehousePermissionBase>();
			// 	var userWarehousePermissionsQuery = uow.Session.QueryOver<UserWarehousePermission>()
			// 		.Where(x => x.User.Id == userId).List();
			// 	userWarehousePermissionsQuery.ForEach(x => permissions.Add(x));
			// 	while(subdivision != null)
			// 	{
			// 		var subdivisionWarehousePermissionQuery = uow.Session.QueryOver<SubdivisionWarehousePermission>()
			// 			.Where(x => x.Subdivision.Id == subdivision.Id).List();
			// 		subdivisionWarehousePermissionQuery.ForEach(x => permissions.Add(x));
			// 		subdivision = subdivision.ParentSubdivision;
			// 	}
			// }
			var warehouse = new List<SubdivisionWarehousePermission>();
			while (subdivision != null)
			{
				var permissions = _subdivisionWarehousePermissions.Where(x =>
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
			=> _subdivisionWarehousePermissions.SingleOrDefault(x =>
					x.Warehouse.Id == warehouseId && x.WarehousePermissionType == warehousePermissionsType).PermissionValue
				.Value;
	}
}
