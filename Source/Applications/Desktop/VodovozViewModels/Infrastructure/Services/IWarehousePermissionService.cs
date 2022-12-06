using Vodovoz.Domain.Permissions.Warehouses;

namespace Vodovoz.Infrastructure.Services
{
	public interface IWarehousePermissionService
	{
		IWarehousePermissionValidator GetValidator();
	}
}
