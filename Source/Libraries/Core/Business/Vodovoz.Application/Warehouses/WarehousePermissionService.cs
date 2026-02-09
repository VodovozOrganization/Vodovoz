using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Application.Warehouses
{
	/// <summary>
	/// Сервис разрешений для складов	/// 
	/// Алгоритм получения прав аналогичен <seealso cref="CurrentWarehousePermissions"/>
	/// </summary>
	internal sealed class WarehousePermissionService : IWarehousePermissionService
	{
		private readonly IGenericRepository<User> _userRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private readonly IGenericRepository<UserWarehousePermission> _userWarehousePermissionRepository;
		private readonly IGenericRepository<SubdivisionWarehousePermission> _subdivisionWarehousePermissionRepository;
		private readonly IGenericRepository<UserSettings> _userSettingsRepository;

		public WarehousePermissionService(
			IGenericRepository<User> userRepository,
			IGenericRepository<Employee> employeeRepository,
			IGenericRepository<UserWarehousePermission> userWarehousePermissionRepository,
			IGenericRepository<SubdivisionWarehousePermission> subdivisionWarehousePermissionRepository,
			IGenericRepository<UserSettings> userSettingsRepository)
		{
			_userRepository = userRepository
				?? throw new ArgumentNullException(nameof(userRepository));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(_employeeRepository));
			_userWarehousePermissionRepository = userWarehousePermissionRepository
				?? throw new ArgumentNullException(nameof(userWarehousePermissionRepository));
			_subdivisionWarehousePermissionRepository = subdivisionWarehousePermissionRepository
				?? throw new ArgumentNullException(nameof(subdivisionWarehousePermissionRepository));
			_userSettingsRepository = userSettingsRepository
				?? throw new ArgumentNullException(nameof(userSettingsRepository));
		}

		public Warehouse GetDefaultWarehouseForUser(IUnitOfWork unitOfWork, int userId)
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

		public IEnumerable<Warehouse> GetAvailableWarehousesForUser(IUnitOfWork unitOfWork, int userId)
		{
			var permissions = GetPermissionsForUser(unitOfWork, userId);
			return permissions
				.Where(x => x.WarehousePermissionType == WarehousePermissionsType.WarehouseView
					&& x.PermissionValue == true)
				.Select(x => x.Warehouse);
		}

		public IEnumerable<Warehouse> GetAvailableWarehousesForUser(IUnitOfWork unitOfWork, int userId, WarehousePermissionsType warehousePermissionsType)
		{
			var permissions = GetPermissionsForUser(unitOfWork, userId);
			return permissions
				.Where(x => x.WarehousePermissionType == warehousePermissionsType
					&& x.PermissionValue == true)
				.Select(x => x.Warehouse);
		}

		private IEnumerable<WarehousePermissionBase> GetPermissionsForUser(IUnitOfWork unitOfWork, int userId)
		{
			var user = _userRepository
				.GetFirstOrDefault(
					unitOfWork,
					x => !x.Deactivated
						&& x.Id == userId);

			if(user is null)
			{
				return Enumerable.Empty<WarehousePermissionBase>();
			}

			var employee = _employeeRepository
				.GetFirstOrDefault(
					unitOfWork,
					x => x.Status != EmployeeStatus.IsFired
						&& x.User.Id == userId);

			if(employee is null)
			{
				return Enumerable.Empty<WarehousePermissionBase>();
			}

			var permissions = new List<WarehousePermissionBase>();

			var userWarehousePermissions = _userWarehousePermissionRepository
				.Get(
					unitOfWork,
					x => x.User.Id == userId);

			permissions.AddRange(userWarehousePermissions);

			var subdivision = employee.Subdivision;

			while(subdivision != null)
			{
				var warehouseIds = permissions
					.Select(x => x.Warehouse.Id)
					.Distinct()
					.ToArray();

				permissions
					.AddRange(_subdivisionWarehousePermissionRepository
						.Get(
							unitOfWork,
							x => x.Subdivision.Id == subdivision.Id
								&& !warehouseIds.Contains(x.Warehouse.Id)));

				subdivision = subdivision.ParentSubdivision;
			}

			return permissions;
		}
	}
}
