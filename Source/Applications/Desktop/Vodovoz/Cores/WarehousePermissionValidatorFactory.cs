using QS.DomainModel.UoW;
using QS.Permissions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Permissions;
namespace Vodovoz.Core
{
	public class WarehousePermissionValidatorFactory : IWarehousePermissionValidatorFactory
	{
		public IWarehousePermissionValidator CreateValidator(int userId)
		{
			PermissionMatrix<WarehousePermissions, Warehouse> permissionMatrix;
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateForRoot<User>(userId)) {
				permissionMatrix = new PermissionMatrix<WarehousePermissions, Warehouse>();
				permissionMatrix.Init();
				permissionMatrix.ParseJson(uow.Root.WarehouseAccess);
			}
			return new WarehousePermissionValidator(permissionMatrix);
		}
	}
}
