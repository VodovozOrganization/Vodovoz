using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions.Warehouses;

namespace Vodovoz.Application.Warehouse
{
	internal sealed class WarehousePermissionService : IWarehousePermissionService
	{
		private readonly IGenericRepository<WarehouseEntity> _warehouseRepository;
		private readonly IGenericRepository<UserSettings> _userSettingsRepository;

		public WarehousePermissionService(
			IGenericRepository<WarehouseEntity> warehouseRepository,
			IGenericRepository<UserWarehousePermission> userWarehousePermissionRepository,
			IGenericRepository<SubdivisionWarehousePermission> subdivisionWarehousePermissionRepository,
			IGenericRepository<UserSettings> userSettingsRepository)
		{
			_warehouseRepository = warehouseRepository
				?? throw new ArgumentNullException(nameof(warehouseRepository));
			_userSettingsRepository = userSettingsRepository
				?? throw new ArgumentNullException(nameof(userSettingsRepository));
		}

		public WarehouseEntity GetDefaultWarehouseForUser(IUnitOfWork unitOfWork, int userId)
		{
			var userSettings = _userSettingsRepository.GetFirstOrDefault(
				unitOfWork,
				us => us.User.Id == userId);

			if(userSettings is null)
			{
				return null;
			}

			return userSettings.DefaultWarehouse;
		}

		public IEnumerable<WarehouseEntity> GetAvailableWarehousesForUser(IUnitOfWork unitOfWork, int userId)
		{
			_warehouseRepository.Get(unitOfWork,
				warehouse => warehouse.WarehousePermissions.Any(
					wp => wp.User.Id == userId && wp.PermissionValue));
		}

		public IEnumerable<WarehouseEntity> GetAvailableWarehousesForUser(IUnitOfWork unitOfWork, int userId, WarehousePermissionsType warehousePermissionsType)
		{
			
		}
	}
}
