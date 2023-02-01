using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ViewModels.Permissions
{
	public class UsersEntityPermissionValuesGetter
	{
		private readonly IPermissionRepository _permissionRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private Dictionary<int, EntityExtendedPermission> _entityExtendedPermissionBySubdivisionsMatrix;

		public UsersEntityPermissionValuesGetter(
			IPermissionRepository permissionRepository,
			ISubdivisionRepository subdivisionRepository)
		{
			_permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
		}
		
		public IList<UserEntityExtendedPermissionNode> GetUsersWithEntityPermission(IUnitOfWork uow, string permissionName)
		{
			return _permissionRepository.GetUsersEntityPermission(uow, permissionName);
		}

		public IEnumerable<UserEntityExtendedPermissionWithSubdivisionNode> GetUsersWithActivePermissionPresetByOwnSubdivision(IUnitOfWork uow, string permissionName) =>
			GetUsersEntityPermissionByOwnSubdivision(uow, permissionName)
				.Where(x => x.HasPermission)
				.ToList();

		private IList<UserEntityExtendedPermissionWithSubdivisionNode> GetUsersEntityPermissionByOwnSubdivision(IUnitOfWork uow, string permissionName)
		{
			_entityExtendedPermissionBySubdivisionsMatrix = new Dictionary<int, EntityExtendedPermission>();
			
			var subdivisionsIds = _subdivisionRepository.GetAllSubdivisionsIds(uow);
			var usersEntityPermissionBySubdivisions = _permissionRepository.GetUsersWithSubdivisionsEntityPermission(uow);

			foreach(var subdivisionId in subdivisionsIds)
			{
				_entityExtendedPermissionBySubdivisionsMatrix.Add(subdivisionId, new EntityExtendedPermission());
			}

			foreach(var item in usersEntityPermissionBySubdivisions)
			{
				var permission = GetEntityPermissionValueBySubdivision(uow, item.Subdivision, permissionName);
				item.CanRead = permission.CanRead;
				item.CanCreate = permission.CanCreate;
				item.CanUpdate = permission.CanUpdate;
				item.CanDelete = permission.CanDelete;
				item.ExtendedPermissionValue = permission.ExtendedPermissionValue;
			}

			return usersEntityPermissionBySubdivisions;
		}
		
		private EntityExtendedPermission GetEntityPermissionValueBySubdivision(IUnitOfWork uow, Subdivision subdivision, string permissionName)
		{
			while(subdivision != null)
			{
				var currentPermission = _entityExtendedPermissionBySubdivisionsMatrix[subdivision.Id];
				if(currentPermission.Initialized && currentPermission.HasPermission)
				{
					return currentPermission;
				}
				
				if(currentPermission.Initialized)
				{
					subdivision = subdivision.ParentSubdivision;
					continue;
				}
				
				var subdivisionPermission = _permissionRepository.GetSubdivisionEntityExtendedPermission(uow, subdivision.Id, permissionName);
				if(subdivisionPermission != null)
				{
					_entityExtendedPermissionBySubdivisionsMatrix[subdivision.Id] = subdivisionPermission;
					return subdivisionPermission;
				}
				
				_entityExtendedPermissionBySubdivisionsMatrix[subdivision.Id].Initialized = true;
				subdivision = subdivision.ParentSubdivision;
			}
			return new EntityExtendedPermission();
		}
	}
}
