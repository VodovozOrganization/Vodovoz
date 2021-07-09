using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Permissions.Warehouse
{
    public class UserWarehousePermissionModel : WarehousePermissionModel
    {
		private IUnitOfWork _uow;
		private User _user;

		public UserWarehousePermissionModel(IUnitOfWork uow, User user)
		{
			this._uow = uow;
			this._user = user;
			AllPermission = GetEnumerator().ToList();
		}

		public override void AddOnUpdatePermission(WarehousePermissions warehousePermission, Store.Warehouse warehouse, bool? permissionValue)
		{
			var findPermission = AllPermission.SingleOrDefault(x =>
				x.Warehouse == warehouse &&
				x.WarehousePermissionType == warehousePermission);

			if(findPermission is null)
			{
				var userWarehousePermission = new UserWarehousePermission
				{
					User = _user,
					TypePermissions = TypePermissions.User,
					Warehouse = warehouse,
					ValuePermission = permissionValue,
					WarehousePermissionType = warehousePermission
				};
				_uow.Save(userWarehousePermission);
			}
			else
			{
				findPermission.ValuePermission = permissionValue;
				_uow.Save(findPermission);
			}
		}

		public override void DeletePermission(WarehousePermissions warehousePermission, Store.Warehouse warehouse)
		{
			var permissionForDelete = AllPermission.SingleOrDefault(x => x.Warehouse == warehouse && x.WarehousePermissionType == warehousePermission);
			if(permissionForDelete != null)
			{
				_uow.Delete(permissionForDelete);
			}
		}

		public override IEnumerable<WarehousePermission> GetEnumerator() => _uow.Session
			.QueryOver<UserWarehousePermission>().Where(x => x.User.Id == _user.Id)
			.List();

		public override List<WarehousePermission> AllPermission { get; set; }
	}
}