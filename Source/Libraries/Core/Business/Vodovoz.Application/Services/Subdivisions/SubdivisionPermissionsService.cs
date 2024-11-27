using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Errors;
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

		//public void AddSubdiviionPermissions(IUnitOfWork uow, Subdivision target, Subdivision source)
		//{
		//	if(!IsCanChangeSubdivisionPermissions(target, out Error error))
		//	{
		//		throw new InvalidOperationException(error.Message);
		//	}

		//	//return AddEntityPermissions(uow, target, source);
		//	//AddPresetPermissions(target, source);
		//	//AddWarehousePermissions(target, source);
		//}

		//public void ReplaceSubdivisionPermissions(IUnitOfWork uow, Subdivision target, Subdivision source)
		//{
		//	if(!IsCanChangeSubdivisionPermissions(target, out Error error))
		//	{
		//		throw new InvalidOperationException(error.Message);
		//	}
		//}

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
				resultPermissions.Add(CreateCopyFromSubdivisionPermissionNode(targetPermission, targetSubdivision));
			}

			foreach(var sourcePermission in sourcePermissions)
			{
				var resultPermissionHavingSameType =
					resultPermissions
					.Select(x => x)
					.FirstOrDefault(x => x.TypeOfEntity == sourcePermission.EntityPermission.TypeOfEntity);

				if(resultPermissionHavingSameType is null)
				{
					resultPermissions.Add(CreateCopyFromSubdivisionPermissionNode(sourcePermission, targetSubdivision));

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
				resultPermissions.Add(CreateCopyFromSubdivisionPermissionNode(sourcePermission, targetSubdivision));
			}

			return resultPermissions;
		}

		private SubdivisionPermissionNode CreateCopyFromSubdivisionPermissionNode(SubdivisionPermissionNode sourceNode, Subdivision targetSubdivision)
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

		private void AddPresetPermissions(IUnitOfWork uow, Subdivision target, Subdivision source)
		{
			var targetPresetPermissions = GetAllPresetPermissionsBySubdivision(uow, target.Id);
			var sourcePresetPermissions = GetAllPresetPermissionsBySubdivision(uow, source.Id);
		}

		private void AddWarehousePermissions(IUnitOfWork uow, Subdivision target, Subdivision source)
		{
			var targetWarehousePermissions = GetAllWarehousePermissionsBySubdivision(uow, target.Id);
			var sourceWarehousePermissions = GetAllWarehousePermissionsBySubdivision(uow, source.Id);
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
