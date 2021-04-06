namespace Vodovoz.Domain.Permissions.Warehouse
{
	public interface IWarehousePermissionValidatorFactory
	{
		IWarehousePermissionValidator CreateValidator(int userId);
	}
}
