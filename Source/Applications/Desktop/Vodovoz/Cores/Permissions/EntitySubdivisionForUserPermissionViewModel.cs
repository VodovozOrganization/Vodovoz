using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Core.Permissions
{
	public class EntitySubdivisionForUserPermissionViewModel
	{
		private readonly IPermissionRepository _permissionRepository = ScopeProvider.Scope.Resolve<IPermissionRepository>();
		private readonly IList<EntitySubdivisionForUserPermission> _deletionPermissionList = new List<EntitySubdivisionForUserPermission>();
		private IList<EntitySubdivisionForUserPermission> _originalPermissionList;
		private IList<TypeOfEntity> _originalTypeOfEntityList;

		public GenericObservableList<EntitySubdivisionForUserPermission> ObservablePermissionsList { get; private set; }
		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntitySubdivisionForUserPermissionViewModel(IUnitOfWork uow, UserBase user)
		{
			Uow = uow;
			User = user;

			_originalPermissionList = _permissionRepository.GetAllSubdivisionForUserEntityPermissions(uow, user.Id);
			ObservablePermissionsList = new GenericObservableList<EntitySubdivisionForUserPermission>(_originalPermissionList.ToList());

			_originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntityOrderedByName(uow);
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(_originalTypeOfEntityList);
		}
		
		public IUnitOfWork Uow { get; }
		public UserBase User { get; }

		public void AddPermission(TypeOfEntity typeOfEntity, Subdivision subdivision)
		{
			if(typeOfEntity == null || subdivision == null || PermissionExists(typeOfEntity, subdivision)) {
				return;
			}

			EntitySubdivisionForUserPermission savedPermission;
			var foundOriginalPermission = _originalPermissionList.FirstOrDefault(x => x.TypeOfEntity == typeOfEntity && x.Subdivision == subdivision);
			if(foundOriginalPermission == null) {
				savedPermission = new EntitySubdivisionForUserPermission() {
					Subdivision = subdivision,
					TypeOfEntity = typeOfEntity,
					User = User
				};
				ObservablePermissionsList.Add(savedPermission);
			} else {
				if(_deletionPermissionList.Contains(foundOriginalPermission)) {
					_deletionPermissionList.Remove(foundOriginalPermission);
				}
				savedPermission = foundOriginalPermission;
				ObservablePermissionsList.Add(savedPermission);
			}
		}

		public void DeletePermission(EntitySubdivisionForUserPermission deletedPermission)
		{
			if(deletedPermission == null) {
				return;
			}
			ObservablePermissionsList.Remove(deletedPermission);
			if(deletedPermission.Id != 0) {
				_deletionPermissionList.Add(deletedPermission);
			}
		}

		public bool PermissionExists(TypeOfEntity type, Subdivision subdivision)
		{
			return ObservablePermissionsList.Any(x => x.TypeOfEntity == type && x.Subdivision == subdivision);
		}

		public void Save()
		{
			foreach(EntitySubdivisionForUserPermission item in ObservablePermissionsList) {
				Uow.Save(item);
			}

			foreach(EntitySubdivisionForUserPermission item in _deletionPermissionList) {
				Uow.Delete(item);
			}
		}

		public void UpdateData(IList<EntitySubdivisionForUserPermission> newEntitySubdivisionForUserPermissions)
		{
			_originalPermissionList = newEntitySubdivisionForUserPermissions;
			ObservablePermissionsList = new GenericObservableList<EntitySubdivisionForUserPermission>(_originalPermissionList.ToList());
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(_originalTypeOfEntityList);
		}

		#region Search
		
		public void SearchPermissions(string searchString)
		{
			//Каждый раз перезаписываем список
			_originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntityOrderedByName(Uow);
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(_originalTypeOfEntityList);
			
			if(searchString != "")
			{
				for(int i = 0; i < ObservableTypeOfEntitiesList.Count; i++)
				{
					//Поиск и удаление не подходящих элементов списка (без учета регистра)
					if (ObservableTypeOfEntitiesList[i].CustomName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) == -1)
					{
						ObservableTypeOfEntitiesList.Remove(ObservableTypeOfEntitiesList[i]);
						i--;
					}
				}
			}
		}

		#endregion
	}
}
