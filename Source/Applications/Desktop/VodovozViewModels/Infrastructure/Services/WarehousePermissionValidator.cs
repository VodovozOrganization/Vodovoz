using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.ViewModels.Infrastructure.Services
{
	public class WarehousePermissionValidator : IWarehousePermissionValidator
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPermissionRepository _permissionRepository;
		
		public WarehousePermissionValidator(IUnitOfWorkFactory unitOfWorkFactory, IPermissionRepository permissionRepository)
		{
			_permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}
		
		public IEnumerable<Warehouse> GetAllowedWarehouses(WarehousePermissionsType permissionType, Employee employee)
		{
			var userId = employee.User.Id;
			var permissions = new List<WarehousePermissionBase>();
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateForRoot<User>(userId))
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

		public bool Validate(WarehousePermissionsType warehousePermissionsType, Warehouse warehouse, Employee employee)
		{
			var hasWar = !(warehouse is null);
			var hasPermission = employee.User.IsAdmin || Validate(employee, warehousePermissionsType, warehouse.Id);
			
			return hasWar && hasPermission;
		}

		public bool Validate(Employee employee, WarehousePermissionsType warehousePermissionsType, int warehouseId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var userWarehousePermission = _permissionRepository.GetUserWarehousePermission(
					uow, employee.User.Id, warehouseId, warehousePermissionsType);

				if(userWarehousePermission?.PermissionValue != null)
				{
					return userWarehousePermission.PermissionValue.Value;
				}

				var subdivision = employee.Subdivision;
				
				while(subdivision != null)
				{
					var subdivisionWarehousePermission = _permissionRepository.GetSubdivisionWarehousePermission(
						uow, subdivision.Id, warehouseId, warehousePermissionsType);
					
					if(subdivisionWarehousePermission?.PermissionValue != null)
					{
						return subdivisionWarehousePermission.PermissionValue.Value;
					}

					subdivision = subdivision.ParentSubdivision;
				}
			}
			
			return false;
		}
	}
}
