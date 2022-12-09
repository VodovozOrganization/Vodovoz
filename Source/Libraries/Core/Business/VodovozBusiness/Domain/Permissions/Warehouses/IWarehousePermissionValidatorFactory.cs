using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Domain.Permissions.Warehouses
{
	public interface IWarehousePermissionValidatorFactory
	{
		IWarehousePermissionValidator CreateValidator(IUnitOfWorkFactory unitOfWorkFactory, IPermissionRepository permissionRepository);
	}
}
