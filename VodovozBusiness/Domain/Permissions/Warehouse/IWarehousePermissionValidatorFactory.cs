namespace Vodovoz.Domain.Permissions.Warehouse
{
	public interface IWarehousePermissionValidatorFactory
	{
		IWarehousePermissionValidator CreateValidator(Subdivision subdivision);
	}
}
