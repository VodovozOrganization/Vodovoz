using QS.DomainModel.UoW;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.ViewModels.Infrastructure.Services
{
	public class WarehousePermissionValidatorFactory : IWarehousePermissionValidatorFactory
	{
		public IWarehousePermissionValidator CreateValidator(
			IUnitOfWorkFactory unitOfWorkFactory, IPermissionRepository permissionRepository)
			=> new WarehousePermissionValidator(unitOfWorkFactory, permissionRepository);
	}
}
