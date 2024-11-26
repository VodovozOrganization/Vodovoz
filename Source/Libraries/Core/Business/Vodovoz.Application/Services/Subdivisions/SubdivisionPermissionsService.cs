using Microsoft.Extensions.Logging;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Permissions;
using VodovozBusiness.Services.Subdivisions;

namespace Vodovoz.Application.Services.Subdivisions
{
	public class SubdivisionPermissionsService : ISubdivisionPermissionsService, IDisposable
	{
		private readonly ILogger<SubdivisionPermissionsService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPermissionRepository _permissionRepository;

		private readonly PermissionExtensionSingletonStore _permissionExtensionSingletonStore;

		public SubdivisionPermissionsService(
			ILogger<SubdivisionPermissionsService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IPermissionRepository permissionRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)))
				.CreateWithoutRoot(nameof(SubdivisionPermissionsService));
			_permissionRepository = permissionRepository ?? throw new System.ArgumentNullException(nameof(permissionRepository));

			_permissionExtensionSingletonStore = PermissionExtensionSingletonStore.GetInstance();
		}

		public void AddSubdiviionPermissions(Subdivision targer, Subdivision source)
		{
			if(targer.HasChildSubdivisions)
			{
				throw new InvalidOperationException("У целевого подразделения не должно быть дочерних подразделений");
			}

			AddEntityPermissions(targer, source);
			AddPresetPermissions(targer, source);
			AddWarehousePermissions(targer, source);
		}

		public void ReplaceSubdivisionPermissions(Subdivision targer, Subdivision source)
		{
			if(targer.HasChildSubdivisions)
			{
				throw new InvalidOperationException("У целевого подразделения не должно быть дочерних подразделений");
			}
		}

		private void AddEntityPermissions(Subdivision targer, Subdivision source)
		{
			var targetEntityPermissions = GetAllEntityPermissionsBySubdivision(targer.Id);
			var sourceEntityPermissions = GetAllEntityPermissionsBySubdivision(source.Id);
		}

		private void AddPresetPermissions(Subdivision targer, Subdivision source)
		{
			var targetPresetPermissions = GetAllPresetPermissionsBySubdivision(targer.Id);
			var sourcePresetPermissions = GetAllPresetPermissionsBySubdivision(source.Id);
		}

		private void AddWarehousePermissions(Subdivision targer, Subdivision source)
		{
			var targetWarehousePermissions = GetAllWarehousePermissionsBySubdivision(targer.Id);
			var sourceWarehousePermissions = GetAllWarehousePermissionsBySubdivision(source.Id);
		}

		private IEnumerable<SubdivisionPermissionNode> GetAllEntityPermissionsBySubdivision(int subdivisionId)
		{
			var entityPermissions =
				_permissionRepository.GetAllSubdivisionEntityPermissions(_unitOfWork, subdivisionId, _permissionExtensionSingletonStore);

			return entityPermissions;
		}

		private IEnumerable<HierarchicalPresetSubdivisionPermission> GetAllPresetPermissionsBySubdivision(int subdivisionId)
		{
			var presetPermissions =
				_permissionRepository.GetAllPresetPermissionsBySubdivision(_unitOfWork, subdivisionId);

			return presetPermissions;
		}

		private IEnumerable<SubdivisionWarehousePermission> GetAllWarehousePermissionsBySubdivision(int subdivisionId)
		{
			var warehousePermissions =
				_permissionRepository.GetAllWarehousePermissionsBySubdivision(_unitOfWork, subdivisionId);

			return warehousePermissions;
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
