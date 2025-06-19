using System;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Domain.Permissions.Warehouses
{
    public class UserWarehousePermissionModel : WarehousePermissionModelBase
    {
		private readonly IUnitOfWork _uow;
		private readonly User _user;

		public UserWarehousePermissionModel(IUnitOfWork uow, User user, IPermissionRepository permissionRepository)
		{
			if(permissionRepository == null)
			{
				throw new ArgumentNullException(nameof(permissionRepository));
			}

			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_user = user ?? throw new ArgumentNullException(nameof(user));
			AllPermission = permissionRepository.GetAllUserWarehousesPermissions(_uow, _user.Id).ToList();
		}

		public override void AddOnUpdatePermission(WarehousePermissionsType warehousePermissionType, Warehouse warehouse, bool? permissionValue)
		{
			var findPermission = AllPermission.SingleOrDefault(x =>
				x.Warehouse == warehouse &&
				x.WarehousePermissionType == warehousePermissionType);

			if(findPermission is null)
			{
				var userWarehousePermission = new UserWarehousePermission
				{
					User = _user,
					PermissionType = PermissionType.User,
					Warehouse = warehouse,
					PermissionValue = permissionValue,
					WarehousePermissionType = warehousePermissionType
				};
				_uow.Save(userWarehousePermission);
			}
			else
			{
				findPermission.PermissionValue = permissionValue;
				_uow.Save(findPermission);
			}
		}

		public override void DeletePermission(WarehousePermissionsType warehousePermissionType, Warehouse warehouse)
		{
			var permissionForDelete = AllPermission.SingleOrDefault(x => x.Warehouse == warehouse && x.WarehousePermissionType == warehousePermissionType);
			if(permissionForDelete != null)
			{
				_uow.Delete(permissionForDelete);
			}
		}

		public override IList<WarehousePermissionBase> AllPermission { get; set; }
	}
}
