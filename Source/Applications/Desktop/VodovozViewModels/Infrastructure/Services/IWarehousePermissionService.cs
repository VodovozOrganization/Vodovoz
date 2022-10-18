using QS.DomainModel.UoW;
using Vodovoz.Domain.Permissions.Warehouses;

namespace Vodovoz.Infrastructure.Services
{
	public interface IWarehousePermissionService
	{
		IWarehousePermissionValidator GetValidator(IUnitOfWork uow, int userId);
		IWarehousePermissionValidator GetValidator(Subdivision subdivision);
	}
}
