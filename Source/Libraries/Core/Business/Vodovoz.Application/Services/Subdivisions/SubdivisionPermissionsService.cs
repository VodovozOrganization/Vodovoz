using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;
using VodovozBusiness.Services.Subdivisions;

namespace Vodovoz.Application.Services.Subdivisions
{
	public class SubdivisionPermissionsService : ISubdivisionPermissionsService
	{
		private readonly IPermissionRepository _permissionRepository;

		private readonly PermissionExtensionSingletonStore _permissionExtensionSingletonStore;

		public SubdivisionPermissionsService(
			IPermissionRepository permissionRepository)
		{
			_permissionRepository = permissionRepository ?? throw new System.ArgumentNullException(nameof(permissionRepository));

			_permissionExtensionSingletonStore = PermissionExtensionSingletonStore.GetInstance();
		}

		public IList<SubdivisionPermissionNode> AddSubdivisionEntityPermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision)
		{
			if(!IsCanChangeSubdivisionPermissions(targetSubdivision, out Error error))
			{
				throw new InvalidOperationException(error.Message);
			}

			var resultPermissions = new List<SubdivisionPermissionNode>();

			var targetPermissions = GetAllEntityPermissionsBySubdivision(uow, targetSubdivision.Id).ToList();
			var sourcePermissions = GetAllEntityPermissionsBySubdivision(uow, sourceSubdivision.Id).ToList();

			foreach(var targetPermission in targetPermissions)
			{
				resultPermissions.Add(CreateCopyOfSubdivisionEntityPermissionNode(targetPermission, targetSubdivision));
			}

			foreach(var sourcePermission in sourcePermissions)
			{
				var resultPermissionHavingSameType =
					resultPermissions
					.Select(x => x)
					.FirstOrDefault(x => x.TypeOfEntity == sourcePermission.EntityPermission.TypeOfEntity);

				if(resultPermissionHavingSameType is null)
				{
					resultPermissions.Add(CreateCopyOfSubdivisionEntityPermissionNode(sourcePermission, targetSubdivision));

					continue;
				}

				resultPermissionHavingSameType.EntitySubdivisionOnlyPermission.CanRead =
					resultPermissionHavingSameType.EntitySubdivisionOnlyPermission.CanRead || sourcePermission.EntityPermission.CanRead;

				resultPermissionHavingSameType.EntitySubdivisionOnlyPermission.CanCreate =
					resultPermissionHavingSameType.EntitySubdivisionOnlyPermission.CanCreate || sourcePermission.EntityPermission.CanCreate;

				resultPermissionHavingSameType.EntitySubdivisionOnlyPermission.CanUpdate =
					resultPermissionHavingSameType.EntitySubdivisionOnlyPermission.CanUpdate || sourcePermission.EntityPermission.CanUpdate;

				resultPermissionHavingSameType.EntitySubdivisionOnlyPermission.CanDelete =
					resultPermissionHavingSameType.EntitySubdivisionOnlyPermission.CanDelete || sourcePermission.EntityPermission.CanDelete;

				foreach(var sourcePermissionExtended in sourcePermission.EntityPermissionExtended)
				{
					var resultPermissionExtended =
						resultPermissionHavingSameType
						.EntityPermissionExtended
						.FirstOrDefault(x => x.PermissionExtendedType == sourcePermissionExtended.PermissionExtendedType);

					if(resultPermissionExtended is null)
					{
						resultPermissionHavingSameType.EntityPermissionExtended.Add(sourcePermissionExtended);
					}

					resultPermissionExtended.IsPermissionAvailable =
						resultPermissionExtended.IsPermissionAvailable == true || sourcePermissionExtended.IsPermissionAvailable == true;
				}
			}

			return resultPermissions;
		}

		public IList<SubdivisionPermissionNode> ReplaceSubdivisionEntityPermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision)
		{
			if(!IsCanChangeSubdivisionPermissions(targetSubdivision, out Error error))
			{
				throw new InvalidOperationException(error.Message);
			}

			var resultPermissions = new List<SubdivisionPermissionNode>();

			var sourcePermissions = GetAllEntityPermissionsBySubdivision(uow, sourceSubdivision.Id).ToList();

			foreach(var sourcePermission in sourcePermissions)
			{
				resultPermissions.Add(CreateCopyOfSubdivisionEntityPermissionNode(sourcePermission, targetSubdivision));
			}

			return resultPermissions;
		}

		public IList<HierarchicalPresetSubdivisionPermission> AddPresetPermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision)
		{
			if(!IsCanChangeSubdivisionPermissions(targetSubdivision, out Error error))
			{
				throw new InvalidOperationException(error.Message);
			}

			var resultPermissions = new List<HierarchicalPresetSubdivisionPermission>();

			var targetPermissions = GetAllPresetPermissionsBySubdivision(uow, targetSubdivision.Id);
			var sourcePermissions = GetAllPresetPermissionsBySubdivision(uow, sourceSubdivision.Id);

			foreach(var targetPermission in targetPermissions)
			{
				resultPermissions.Add(CreateCopyOfSubdivisionPresetPermission(targetPermission, targetSubdivision));
			}

			foreach(var sourcePermission in sourcePermissions)
			{
				var resultPermissionHavingSameType =
					resultPermissions
					.Select(x => x)
					.FirstOrDefault(x => x.PermissionName == sourcePermission.PermissionName);

				if(resultPermissionHavingSameType is null)
				{
					resultPermissions.Add(CreateCopyOfSubdivisionPresetPermission(sourcePermission, targetSubdivision));

					continue;
				}

				resultPermissionHavingSameType.Value =
					resultPermissionHavingSameType.Value || sourcePermission.Value;
			}

			return resultPermissions;
		}

		public IList<HierarchicalPresetSubdivisionPermission> ReplacePresetPermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision)
		{
			if(!IsCanChangeSubdivisionPermissions(targetSubdivision, out Error error))
			{
				throw new InvalidOperationException(error.Message);
			}

			var resultPermissions = new List<HierarchicalPresetSubdivisionPermission>();

			var sourcePermissions = GetAllPresetPermissionsBySubdivision(uow, sourceSubdivision.Id);

			foreach(var sourcePermission in sourcePermissions)
			{
				resultPermissions.Add(CreateCopyOfSubdivisionPresetPermission(sourcePermission, targetSubdivision));
			}

			return resultPermissions;
		}

		public IList<SubdivisionWarehousePermission> AddWarehousePermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision)
		{
			if(!IsCanChangeSubdivisionPermissions(targetSubdivision, out Error error))
			{
				throw new InvalidOperationException(error.Message);
			}

			var targetPermissions = GetAllWarehousePermissionsBySubdivision(uow, targetSubdivision.Id);
			var sourcePermissions = GetAllWarehousePermissionsBySubdivision(uow, sourceSubdivision.Id);

			var resultPermissions = new List<SubdivisionWarehousePermission>();

			foreach(var permission in targetPermissions)
			{
				resultPermissions.Add(CreateCopyOfSubdivisionWarehousePermission(permission, targetSubdivision));
			}

			foreach(var permission in sourcePermissions)
			{
				var existingResultPermission =
					resultPermissions
					.Where(x => x.WarehousePermissionType == permission.WarehousePermissionType
						&& x.Warehouse.Id == permission.Warehouse.Id)
					.FirstOrDefault();

				if(existingResultPermission is null)
				{
					resultPermissions.Add(CreateCopyOfSubdivisionWarehousePermission(permission, targetSubdivision));

					continue;
				}

				if(existingResultPermission.PermissionValue is null && permission.PermissionValue != null)
				{
					continue;
				}

				existingResultPermission.PermissionValue =
					existingResultPermission.PermissionValue == true || permission.PermissionValue == true;
			}

			return resultPermissions;
		}

		public IList<SubdivisionWarehousePermission> ReplaceWarehousePermissions(IUnitOfWork uow, Subdivision targetSubdivision, Subdivision sourceSubdivision)
		{
			if(!IsCanChangeSubdivisionPermissions(targetSubdivision, out Error error))
			{
				throw new InvalidOperationException(error.Message);
			}

			var sourcePermissions = GetAllWarehousePermissionsBySubdivision(uow, sourceSubdivision.Id);

			var resultPermissions = new List<SubdivisionWarehousePermission>();

			foreach(var permission in sourcePermissions)
			{
				resultPermissions.Add(CreateCopyOfSubdivisionWarehousePermission(permission, targetSubdivision));
			}

			return resultPermissions;
		}

		private HierarchicalPresetSubdivisionPermission CreateCopyOfSubdivisionPresetPermission(
			HierarchicalPresetSubdivisionPermission sourcePermission,
			Subdivision targetSubdivision)
		{
			var newPermission = new HierarchicalPresetSubdivisionPermission
			{
				Subdivision = targetSubdivision,
				PermissionName = sourcePermission.PermissionName,
				Value = sourcePermission.Value
			};

			return newPermission;
		}

		private SubdivisionWarehousePermission CreateCopyOfSubdivisionWarehousePermission(SubdivisionWarehousePermission sourcePermission, Subdivision targetSubdivision)
		{
			var newPermission = new SubdivisionWarehousePermission
			{
				Subdivision = targetSubdivision,
				PermissionType = PermissionType.Subdivision,
				WarehousePermissionType = sourcePermission.WarehousePermissionType,
				Warehouse = sourcePermission.Warehouse,
				PermissionValue = sourcePermission.PermissionValue
			};

			return newPermission;
		}

		private SubdivisionPermissionNode CreateCopyOfSubdivisionEntityPermissionNode(SubdivisionPermissionNode sourceNode, Subdivision targetSubdivision)
		{
			var sourceEntityPermissions = sourceNode.EntityPermission;
			var typeOfEntity = sourceNode.EntitySubdivisionOnlyPermission.TypeOfEntity;

			var copyNode = new SubdivisionPermissionNode
			{
				EntitySubdivisionOnlyPermission = new EntitySubdivisionOnlyPermission
				{
					Subdivision = targetSubdivision
				}
			};

			copyNode.CopySettingsFromNodeExceptSubdivision(sourceNode);

			return copyNode;
		}

		private IEnumerable<SubdivisionPermissionNode> GetAllEntityPermissionsBySubdivision(IUnitOfWork uow, int subdivisionId)
		{
			var entityPermissions =
				_permissionRepository.GetAllSubdivisionEntityPermissions(uow, subdivisionId, _permissionExtensionSingletonStore);

			return entityPermissions;
		}

		private IEnumerable<HierarchicalPresetSubdivisionPermission> GetAllPresetPermissionsBySubdivision(IUnitOfWork uow, int subdivisionId)
		{
			var presetPermissions =
				_permissionRepository.GetAllPresetPermissionsBySubdivision(uow, subdivisionId);

			return presetPermissions;
		}

		private IEnumerable<SubdivisionWarehousePermission> GetAllWarehousePermissionsBySubdivision(IUnitOfWork uow, int subdivisionId)
		{
			var warehousePermissions =
				_permissionRepository.GetAllWarehousePermissionsBySubdivision(uow, subdivisionId);

			return warehousePermissions;
		}

		private bool IsCanChangeSubdivisionPermissions(Subdivision target, out Error error)
		{
			if(!IsTargetSubdivisionNotNull(target, out error))
			{
				return false;
			}

			if(!IsTargetSubdivisionHasNotChildSubdivisions(target, out error))
			{
				return false;
			}

			return true;
		}

		private bool IsTargetSubdivisionNotNull(Subdivision target, out Error error)
		{
			error = null;

			if(target is null)
			{
				error =
					new Error(typeof(Subdivision), nameof(target), "Целевое подразделение должно быть установлено");

				return false;
			}

			return true;
		}

		private bool IsTargetSubdivisionHasNotChildSubdivisions(Subdivision target, out Error error)
		{
			error = null;

			if(target.HasChildSubdivisions)
			{
				error =
					new Error(typeof(Subdivision), nameof(target), "У целевого подразделения не должно быть дочерних подразделений");

				return false;
			}

			return true;
		}
	}
}
