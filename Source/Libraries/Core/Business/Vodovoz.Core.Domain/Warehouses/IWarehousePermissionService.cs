using QS.DomainModel.UoW;
using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Warehouses
{
	/// <summary>
	/// Сервис прав доступа к складам
	/// </summary>
	public interface IWarehousePermissionService
	{
		/// <summary>
		/// Возвращает доступные для пользователя склады
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="userId">Id пользователя</param>
		/// <returns>Список складов</returns>
		IEnumerable<Warehouse> GetAvailableWarehousesForUser(IUnitOfWork unitOfWork, int userId);

		/// <summary>
		/// Возвращает доступные для пользователя склады с уканным типом права
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="userId">Id пользователя</param>
		/// <param name="warehousePermissionsType">Тип права на склад</param>
		/// <returns>Список складов</returns>
		IEnumerable<Warehouse> GetAvailableWarehousesForUser(IUnitOfWork unitOfWork, int userId, WarehousePermissionsType warehousePermissionsType);

		/// <summary>
		/// Возвращает склад пользователя по умолчанию
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="userId">Id пользователя</param>
		/// <returns>Склад</returns>
		Warehouse GetDefaultWarehouseForUser(IUnitOfWork unitOfWork, int userId);
	}
}
