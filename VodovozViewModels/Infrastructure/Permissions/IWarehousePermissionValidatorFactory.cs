using System;
using QS.DomainModel.Entity;
namespace Vodovoz.Infrastructure.Permissions
{
	public interface IWarehousePermissionValidatorFactory
	{
		IWarehousePermissionValidator CreateValidator(int userId);
	}
}
