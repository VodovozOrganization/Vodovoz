using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Permissions.Warehouses
{
    public class UserWarehousePermissionModelBase : WarehousePermissionModelBase
    {
		private IUnitOfWork _uow;
		private User _user;

		public UserWarehousePermissionModelBase(IUnitOfWork uow, User user)
		{
			_uow = uow;
			_user = user;
			AllPermission = _uow.Session
				.QueryOver<UserWarehousePermission>().Where(x => x.User.Id == _user.Id)
				.List().ToList<WarehousePermissionBase>();
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

		public override List<WarehousePermissionBase> AllPermission { get; set; }
	}
}
