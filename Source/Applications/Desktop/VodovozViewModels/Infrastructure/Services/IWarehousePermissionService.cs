using Vodovoz.Domain.Permissions.Warehouses;

namespace Vodovoz.Infrastructure.Services
{
	/// <summary>
	/// Сервис прав доступа к складам
	/// </summary>
	public interface IWarehousePermissionService
	{
		/// <summary>
		/// Возвращает валидатор прав доступа к складам
		/// </summary>
		/// <returns></returns>
		IWarehousePermissionValidator GetValidator();
	}
}
