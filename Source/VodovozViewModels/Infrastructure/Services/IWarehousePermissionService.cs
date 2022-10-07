using System;
using Vodovoz.Infrastructure.Permissions;

namespace Vodovoz.Infrastructure.Services
{
	public interface IWarehousePermissionService
	{
		IWarehousePermissionValidator GetValidator(int userId);
	}
}
