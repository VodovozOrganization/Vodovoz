using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ViewModels.Permissions
{
	public class UsersPresetPermissionValuesGetter
	{
		private readonly IPermissionRepository _permissionRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private Dictionary<int, PermissionValue> _presetPermissionBySubdivisionsMatrix;

		public UsersPresetPermissionValuesGetter(
			IPermissionRepository permissionRepository,
			ISubdivisionRepository subdivisionRepository)
		{
			_permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
		}
		
		public IList<UserNode> GetUsersWithActivePermission(IUnitOfWork uow, string permissionName)
		{
			return _permissionRepository.GetUsersWithActivePermission(uow, permissionName);
		}

		public IEnumerable<UserPresetPermissionWithSubdivisionNode> GetUsersWithActivePermissionPresetByOwnSubdivision(IUnitOfWork uow, string permissionName) =>
			GetUsersPermissionPresetByOwnSubdivision(uow, permissionName)
				.Where(x => x.PermissionValue != null && x.PermissionValue.Value)
				.ToList();

		private IList<UserPresetPermissionWithSubdivisionNode> GetUsersPermissionPresetByOwnSubdivision(IUnitOfWork uow, string permissionName)
		{
			_presetPermissionBySubdivisionsMatrix = new Dictionary<int, PermissionValue>();
			
			var subdivisionsIds = _subdivisionRepository.GetAllSubdivisionsIds(uow);
			var usersPresetPermissionBySubdivisions = _permissionRepository.GetUsersWithSubdivisionsPresetPermission(uow);

			foreach(var subdivisionId in subdivisionsIds)
			{
				_presetPermissionBySubdivisionsMatrix.Add(subdivisionId, PermissionValue.Unknown);
			}

			foreach(var item in usersPresetPermissionBySubdivisions)
			{
				item.PermissionValue = GetPresetPermissionValueBySubdivision(uow, item.Subdivision, permissionName);
			}

			return usersPresetPermissionBySubdivisions;
		}
		
		private bool GetPresetPermissionValueBySubdivision(IUnitOfWork uow, Subdivision subdivision, string permissionName)
		{
			while(subdivision != null)
			{
				var currentPermission = _presetPermissionBySubdivisionsMatrix[subdivision.Id];
				if(currentPermission != PermissionValue.Unknown && currentPermission != PermissionValue.Null)
				{
					return Convert.ToBoolean(_presetPermissionBySubdivisionsMatrix[subdivision.Id]);
				}
				
				if(currentPermission == PermissionValue.Null)
				{
					subdivision = subdivision.ParentSubdivision;
					continue;
				}
				
				var subdivisionPermission = _permissionRepository.GetPresetSubdivisionPermission(uow, subdivision, permissionName);
				if(subdivisionPermission != null)
				{
					_presetPermissionBySubdivisionsMatrix[subdivision.Id] = (PermissionValue)Convert.ToInt32(subdivisionPermission.Value);
					return subdivisionPermission.Value;
				}
				
				_presetPermissionBySubdivisionsMatrix[subdivision.Id] = PermissionValue.Null;
				subdivision = subdivision.ParentSubdivision;
			}
			return false;
		}
	}
	
	public enum PermissionValue
	{
		Unknown = -1,
		False = 0,
		True = 1,
		Null = 2
	}
}
