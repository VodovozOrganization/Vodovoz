using QS.DomainModel.UoW;
using QSOrmProject.Permissions;
using QSProjectsLib;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;

namespace Vodovoz.Core
{
	public static class CurrentPermissions
	{
		static PermissionMatrix<WarehousePermissions, Warehouse> warehouse;

		public static PermissionMatrix<WarehousePermissions, Warehouse> Warehouse{
			get{
				if (warehouse == null)
					Load();
				return warehouse;
			}
		}

		private static void Load()
		{
			using(var uow = UnitOfWorkFactory.CreateForRoot<User>(QSMain.User.Id))
			{
				warehouse = new PermissionMatrix<WarehousePermissions, Domain.Store.Warehouse>();
				warehouse.Init();
				warehouse.ParseJson(uow.Root.WarehouseAccess);
			}
		}
	}
}
