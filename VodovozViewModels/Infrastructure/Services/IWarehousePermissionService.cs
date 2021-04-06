using Vodovoz.Domain.Permissions.Warehouse;

namespace Vodovoz.Infrastructure.Services
{
	public interface IWarehousePermissionService
	{
		IWarehousePermissionValidator GetValidator(int userId);
	}
}
