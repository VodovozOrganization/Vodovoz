using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Core
{
	public class WarehousePermissionValidator : IWarehousePermissionValidator
	{
		private readonly IEnumerable<WarehousePermissionBase> _subdivisionWarehousePermissions;

		public WarehousePermissionValidator(IEnumerable<WarehousePermissionBase> subdivisionWarehousePermissions)
		{
			_subdivisionWarehousePermissions = subdivisionWarehousePermissions;
		}

		public IEnumerable<Warehouse> GetAllowedWarehouses(WarehousePermissionsType permissionType, Employee employee)
		{
			var userId = employee.User.Id;
			var permissions = new List<WarehousePermissionBase>();
			using(var uow = UnitOfWorkFactory.CreateForRoot<User>(userId))
			{
			 	var subdivision = employee.Subdivision;
			 	permissions = new List<WarehousePermissionBase>();
			 	var userWarehousePermissionsQuery = uow.Session.QueryOver<UserWarehousePermission>()
			 		.Where(x => x.User.Id == userId).List();
			 	userWarehousePermissionsQuery.ForEach(x => permissions.Add(x));
			 	while(subdivision != null)
			 	{
			 		var subdivisionWarehousePermissionQuery = uow.Session.QueryOver<SubdivisionWarehousePermission>()
			 			.Where(x => x.Subdivision.Id == subdivision.Id).List();
			 		subdivisionWarehousePermissionQuery.ForEach(x => permissions.Add(x));
			 		subdivision = subdivision.ParentSubdivision;
			 	}
			}
			return permissions.GroupBy(x => x.Warehouse.Id).Where(x=>x.First().PermissionValue == true).Select(x => x.First().Warehouse);
		}

		public bool Validate(WarehousePermissionsType warehousePermissionsType, Warehouse warehouse, User user)
		{
			return !(warehouse is null) && (user.IsAdmin || Validate(warehousePermissionsType, warehouse.Id));
		}

		public bool Validate(WarehousePermissionsType warehousePermissionsType, int warehouseId)
			=> _subdivisionWarehousePermissions.SingleOrDefault(x =>
					x.Warehouse.Id == warehouseId && x.WarehousePermissionType == warehousePermissionsType).PermissionValue
				.Value;
	}
}
