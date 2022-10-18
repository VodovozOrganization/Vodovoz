namespace Vodovoz.Domain.Permissions.Warehouses
{
	public interface IWarehousePermissionValidatorFactory
	{
		IWarehousePermissionValidator CreateValidator(Subdivision subdivision);
	}
}
