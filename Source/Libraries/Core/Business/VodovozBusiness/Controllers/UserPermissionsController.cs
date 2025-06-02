using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Controllers
{
	public class UserPermissionsController : IUserPermissionsController
	{
		private readonly IPermissionRepository _permissionRepository;
		private readonly ILifetimeScope _scope;

		public UserPermissionsController(IPermissionRepository permissionRepository, ILifetimeScope scope)
		{
			_permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			
			NewUserWarehousesPermissions = new List<WarehousePermissionBase>();
			NewUserPresetPermissions = new List<HierarchicalPresetPermissionBase>();
			NewEntityUserPermissions = new List<EntityUserPermission>();
			NewEntityUserPermissionsExtended = new List<EntityUserPermissionExtended>();
			NewEntitySubdivisionForUserPermissions = new List<EntitySubdivisionForUserPermission>();
		}

		public IList<WarehousePermissionBase> NewUserWarehousesPermissions { get; private set; }
		public IList<HierarchicalPresetPermissionBase> NewUserPresetPermissions { get; private set; }
		public IList<EntityUserPermission> NewEntityUserPermissions { get; private set; }
		public IList<EntityUserPermissionExtended> NewEntityUserPermissionsExtended { get; private set; }
		public IList<EntitySubdivisionForUserPermission> NewEntitySubdivisionForUserPermissions { get; private set; }

		public void AddingPermissionsToUser(IUnitOfWork uow, int fromUserId, int toUserId)
		{
			AddingWarehousesPermissions(uow, fromUserId, toUserId);
			AddingPresetPermissions(uow, fromUserId, toUserId);
			AddingEntityPermissions(uow, fromUserId, toUserId);
			AddingSubdivisionForUserPermissions(uow, fromUserId, toUserId);
		}
		
		public void ChangePermissionsFromUser(IUnitOfWork uow, int fromUserId, int toUserId)
		{
			ReplacementWarehousesPermissions(uow, fromUserId, toUserId);
			ReplacementPresetPermissions(uow, fromUserId, toUserId);
			ReplacementEntityPermissions(uow, fromUserId, toUserId);
			ReplacementSubdivisionForUserPermissions(uow, fromUserId, toUserId);
		}
		
		public IList<UserPermissionNode> GetAllNewEntityUserPermissions()
		{
			var newPermissions = new List<UserPermissionNode>();
			var permissionExtensions = PermissionExtensionSingletonStore.GetInstance().PermissionExtensions;

			foreach(var permission in NewEntityUserPermissions)
			{
				var node = _scope.Resolve<UserPermissionNode>();
				node.EntityUserOnlyPermission = permission;
				node.TypeOfEntity = permission.TypeOfEntity;
				node.EntityPermissionExtended = new List<EntityUserPermissionExtended>();

				foreach(var extension in permissionExtensions)
				{
					var extendedPermission = NewEntityUserPermissionsExtended.FirstOrDefault(
						x => x.User.Id == permission.User.Id
							&& x.TypeOfEntity.Id == permission.TypeOfEntity.Id
							&& x.PermissionId == extension.PermissionId);

					if(extendedPermission != null)
					{
						node.EntityPermissionExtended.Add(extendedPermission);
						continue;
					}

					extendedPermission = _scope.Resolve<EntityUserPermissionExtended>();
					extendedPermission.IsPermissionAvailable = null;
					extendedPermission.PermissionId = extension.PermissionId;
					extendedPermission.User = permission.User;
					extendedPermission.TypeOfEntity = permission.TypeOfEntity;
					
					node.EntityPermissionExtended.Add(extendedPermission);
				}
				newPermissions.Add(node);
			}

			return newPermissions;
		}

		#region Добавление прав

		private void AddingWarehousesPermissions(IUnitOfWork uow, int fromUserId, int toUserId)
		{
			var addedWarehousesPermissions = _permissionRepository.GetAllUserWarehousesPermissions(uow, fromUserId);

			if(!NewUserWarehousesPermissions.Any())
			{
				NewUserWarehousesPermissions =
					new List<WarehousePermissionBase>(_permissionRepository.GetAllUserWarehousesPermissions(uow, toUserId));
			}

			foreach(var permission in addedWarehousesPermissions)
			{
				CheckAndSetWarehousesPermission(toUserId, permission);
			}
		}

		private void CheckAndSetWarehousesPermission(int toUserId, WarehousePermissionBase permission)
		{
			var userPermission = NewUserWarehousesPermissions.SingleOrDefault(
				x => x.PermissionType == PermissionType.User
					&& x.Warehouse.Id == permission.Warehouse.Id
					&& x.WarehousePermissionType == permission.WarehousePermissionType);

			if(userPermission != null)
			{
				if(userPermission.PermissionValue.HasValue
					&& permission.PermissionValue.HasValue
					&& !userPermission.PermissionValue.Value
					&& permission.PermissionValue.Value)
				{
					userPermission.PermissionValue = permission.PermissionValue;
				}
			}
			else
			{
				NewUserWarehousesPermissions.Add(CreateAndAddWarehousePermission(toUserId, permission));
			}
		}

		private void AddingPresetPermissions(IUnitOfWork uow, int fromUserId, int toUserId)
		{
			var addedPresetPermissions = _permissionRepository.GetAllPresetUserPermission(uow, fromUserId);

			if(!NewUserPresetPermissions.Any())
			{
				NewUserPresetPermissions =
					new List<HierarchicalPresetPermissionBase>(_permissionRepository.GetAllPresetUserPermissionBase(uow, toUserId));
			}

			foreach(var permission in addedPresetPermissions)
			{
				CheckAndSetPresetPermission(toUserId, permission);
			}
		}

		private void CheckAndSetPresetPermission(int toUserId, HierarchicalPresetUserPermission permission)
		{
			var userPermission =
				NewUserPresetPermissions.SingleOrDefault(x => x.PermissionName == permission.PermissionName);
			if(userPermission != null)
			{
				if(permission.Value && !userPermission.Value)
				{
					userPermission.Value = true;
				}
			}
			else
			{
				NewUserPresetPermissions.Add(CreateAndAddHierarchicalPresetUserPermission(toUserId, permission));
			}
		}

		private void AddingEntityPermissions(IUnitOfWork uow, int fromUserId, int toUserId)
		{
			var addedEntityPermissions = _permissionRepository.GetAllEntityUserPermissions(uow, fromUserId);
			var addedEntityPermissionsExtended = _permissionRepository.GetAllEntityUserPermissionsExtended(uow, fromUserId);

			if(!NewEntityUserPermissions.Any())
			{
				NewEntityUserPermissions = new List<EntityUserPermission>(_permissionRepository.GetAllEntityUserPermissions(uow, toUserId));
			}
			if(!NewEntityUserPermissionsExtended.Any())
			{
				NewEntityUserPermissionsExtended =
					new List<EntityUserPermissionExtended>(_permissionRepository.GetAllEntityUserPermissionsExtended(uow, toUserId));
			}
			
			foreach(var permission in addedEntityPermissions)
			{
				CheckAndSetEntityPermission(toUserId, permission);
			}
			foreach(var permission in addedEntityPermissionsExtended)
			{
				CheckAndSetEntityPermissionExtended(toUserId, permission);
			}
		}
		
		private void CheckAndSetEntityPermission(int toUserId, EntityUserPermission permission)
		{
			var userPermission = NewEntityUserPermissions.SingleOrDefault(x => x.TypeOfEntity.Id == permission.TypeOfEntity.Id);
			if(userPermission != null)
			{
				userPermission.CanRead = CheckValueAndSet(userPermission.CanRead, permission.CanRead);
				userPermission.CanCreate = CheckValueAndSet(userPermission.CanCreate, permission.CanCreate);
				userPermission.CanUpdate = CheckValueAndSet(userPermission.CanUpdate, permission.CanUpdate);
				userPermission.CanDelete = CheckValueAndSet(userPermission.CanDelete, permission.CanDelete);
			}
			else
			{
				NewEntityUserPermissions.Add(CreateAndAddEntityUserPermission(toUserId, permission));
			}
		}

		private void CheckAndSetEntityPermissionExtended(int toUserId, EntityUserPermissionExtended permission)
		{
			var userPermission = NewEntityUserPermissionsExtended.SingleOrDefault(
				x => x.PermissionExtendedType == PermissionExtendedType.User
					&& x.TypeOfEntity.Id == permission.TypeOfEntity.Id
					&& x.PermissionId == permission.PermissionId);

			if(userPermission != null)
			{
				if(userPermission.IsPermissionAvailable.HasValue
					&& permission.IsPermissionAvailable.HasValue
					&& !userPermission.IsPermissionAvailable.Value
					&& permission.IsPermissionAvailable.Value)
				{
					userPermission.IsPermissionAvailable = permission.IsPermissionAvailable;
				}
			}
			else
			{
				NewEntityUserPermissionsExtended.Add(CreateAndAddEntityUserPermissionExtended(toUserId, permission));
			}
		}

		private void AddingSubdivisionForUserPermissions(IUnitOfWork uow, int fromUserId, int toUserId)
		{
			var addedPresetPermissions = _permissionRepository.GetAllSubdivisionForUserEntityPermissions(uow, fromUserId);

			if(!NewEntitySubdivisionForUserPermissions.Any())
			{
				NewEntitySubdivisionForUserPermissions = new List<EntitySubdivisionForUserPermission>(
					_permissionRepository.GetAllSubdivisionForUserEntityPermissions(uow, toUserId));
			}
			
			foreach(var permission in addedPresetPermissions)
			{
				CheckAndSetSubdivisionForUserPermission(toUserId, permission);
			}
		}

		private void CheckAndSetSubdivisionForUserPermission(int toUserId, EntitySubdivisionForUserPermission permission)
		{
			var userPermission = NewEntitySubdivisionForUserPermissions.SingleOrDefault(
				x => x.TypeOfEntity.Id == permission.TypeOfEntity.Id
					&& x.Subdivision.Id == permission.Subdivision.Id);

			if(userPermission != null)
			{
				userPermission.CanRead = CheckValueAndSet(userPermission.CanRead, permission.CanRead);
				userPermission.CanCreate = CheckValueAndSet(userPermission.CanCreate, permission.CanCreate);
				userPermission.CanUpdate = CheckValueAndSet(userPermission.CanUpdate, permission.CanUpdate);
				userPermission.CanDelete = CheckValueAndSet(userPermission.CanDelete, permission.CanDelete);
			}
			else
			{
				NewEntitySubdivisionForUserPermissions.Add(CreateAndAddEntitySubdivisionForUserPermission(toUserId, permission));
			}
		}
		
		private bool CheckValueAndSet(bool userValue, bool addingValue)
		{
			if(!userValue && addingValue)
			{
				userValue = addingValue;
			}

			return userValue;
		}

		#endregion

		#region Замена прав

		private void ReplacementWarehousesPermissions(IUnitOfWork uow, int fromUserId, int toUserId)
		{
			var addedWarehousesPermissions = _permissionRepository.GetAllUserWarehousesPermissions(uow, fromUserId);
			var removedWarehousesPermissions = _permissionRepository.GetAllUserWarehousesPermissions(uow, toUserId);

			foreach(var permission in removedWarehousesPermissions)
			{
				uow.Delete(permission);
			}
			
			NewUserWarehousesPermissions.Clear();

			foreach(var permission in addedWarehousesPermissions)
			{
				NewUserWarehousesPermissions.Add(CreateAndAddWarehousePermission(toUserId, permission));
			}
		}
		
		private void ReplacementPresetPermissions(IUnitOfWork uow, int fromUserId, int toUserId)
		{
			var addedPresetPermissions = _permissionRepository.GetAllPresetUserPermission(uow, fromUserId);
			var removedPresetPermissions = _permissionRepository.GetAllPresetUserPermission(uow, toUserId);

			foreach(var permission in removedPresetPermissions)
			{
				var userPermission = addedPresetPermissions.SingleOrDefault(x => x.PermissionName == permission.PermissionName);

				if(userPermission == null)
				{
					uow.Delete(permission);
				}
			}

			NewUserPresetPermissions.Clear();

			foreach(var permission in addedPresetPermissions)
			{
				var userPermission = removedPresetPermissions.SingleOrDefault(x => x.PermissionName == permission.PermissionName);
				NewUserPresetPermissions.Add(userPermission ?? CreateAndAddHierarchicalPresetUserPermission(toUserId, permission));
			}
		}
		
		private void ReplacementEntityPermissions(IUnitOfWork uow, int fromUserId, int toUserId)
		{
			var addedEntityPermissions = _permissionRepository.GetAllEntityUserPermissions(uow, fromUserId);
			var addedEntityPermissionsExtended = _permissionRepository.GetAllEntityUserPermissionsExtended(uow, fromUserId);
			var removedEntityPermissions = _permissionRepository.GetAllEntityUserPermissions(uow, toUserId);
			var removedEntityPermissionsExtended = _permissionRepository.GetAllEntityUserPermissionsExtended(uow, toUserId);

			foreach(var permission in removedEntityPermissions)
			{
				uow.Delete(permission);
			}
			foreach(var permission in removedEntityPermissionsExtended)
			{
				uow.Delete(permission);
			}
			
			NewEntityUserPermissions.Clear();
			NewEntityUserPermissionsExtended.Clear();

			foreach(var permission in addedEntityPermissions)
			{
				NewEntityUserPermissions.Add(CreateAndAddEntityUserPermission(toUserId, permission));
			}
			foreach(var permission in addedEntityPermissionsExtended)
			{
				NewEntityUserPermissionsExtended.Add(CreateAndAddEntityUserPermissionExtended(toUserId, permission));
			}
		}

		private void ReplacementSubdivisionForUserPermissions(IUnitOfWork uow, int fromUserId, int toUserId)
		{
			var addedPresetPermissions = _permissionRepository.GetAllSubdivisionForUserEntityPermissions(uow, fromUserId);
			var removedPresetPermissions = _permissionRepository.GetAllSubdivisionForUserEntityPermissions(uow, toUserId);

			foreach(var permission in removedPresetPermissions)
			{
				uow.Delete(permission);
			}
			
			NewEntitySubdivisionForUserPermissions.Clear();

			foreach(var permission in addedPresetPermissions)
			{
				NewEntitySubdivisionForUserPermissions.Add(CreateAndAddEntitySubdivisionForUserPermission(toUserId, permission));
			}
		}

		#endregion

		#region Создание прав

		private WarehousePermissionBase CreateAndAddWarehousePermission(int toUserId, WarehousePermissionBase permission)
		{
			var userWarehousePermission = _scope.Resolve<UserWarehousePermission>();
			userWarehousePermission.User = _scope.Resolve<User>();
			userWarehousePermission.User.Id = toUserId;
			userWarehousePermission.PermissionType = PermissionType.User;
			userWarehousePermission.Warehouse = permission.Warehouse;
			userWarehousePermission.PermissionValue = permission.PermissionValue;
			userWarehousePermission.WarehousePermissionType = permission.WarehousePermissionType;

			return userWarehousePermission;
		}

		private HierarchicalPresetPermissionBase CreateAndAddHierarchicalPresetUserPermission(
			int toUserId, HierarchicalPresetUserPermission permission)
		{
			var userPresetPermission = _scope.Resolve<HierarchicalPresetUserPermission>();
			userPresetPermission.User = _scope.Resolve<User>();
			userPresetPermission.User.Id = toUserId;
			userPresetPermission.PresetPermissionType = PresetPermissionType.user;
			userPresetPermission.PermissionName = permission.PermissionName;
			userPresetPermission.Value = permission.Value;

			return userPresetPermission;
		}

		private EntityUserPermission CreateAndAddEntityUserPermission(int toUserId, EntityUserPermission permission)
		{
			var entityUserPermission = _scope.Resolve<EntityUserPermission>();
			entityUserPermission.User = _scope.Resolve<User>();
			entityUserPermission.User.Id = toUserId;
			entityUserPermission.TypeOfEntity = permission.TypeOfEntity;
			entityUserPermission.CanCreate = permission.CanCreate;
			entityUserPermission.CanRead = permission.CanRead;
			entityUserPermission.CanUpdate = permission.CanUpdate;
			entityUserPermission.CanDelete = permission.CanDelete;

			return entityUserPermission;
		}
		
		private EntityUserPermissionExtended CreateAndAddEntityUserPermissionExtended(int toUserId, EntityUserPermissionExtended permission)
		{
			var entityUserPermissionExtended = _scope.Resolve<EntityUserPermissionExtended>();
			entityUserPermissionExtended.User = _scope.Resolve<User>();
			entityUserPermissionExtended.User.Id = toUserId;
			entityUserPermissionExtended.PermissionExtendedType = PermissionExtendedType.User;
			entityUserPermissionExtended.TypeOfEntity = permission.TypeOfEntity;
			entityUserPermissionExtended.IsPermissionAvailable = permission.IsPermissionAvailable;
			entityUserPermissionExtended.PermissionId = permission.PermissionId;

			return entityUserPermissionExtended;
		}
		
		private EntitySubdivisionForUserPermission CreateAndAddEntitySubdivisionForUserPermission(int toUserId,
			EntitySubdivisionForUserPermission permission)
		{
			var entitySubdivisionForUserPermission = _scope.Resolve<EntitySubdivisionForUserPermission>();
			entitySubdivisionForUserPermission.User = _scope.Resolve<User>();
			entitySubdivisionForUserPermission.User.Id = toUserId;
			entitySubdivisionForUserPermission.TypeOfEntity = permission.TypeOfEntity;
			entitySubdivisionForUserPermission.Subdivision = permission.Subdivision;
			entitySubdivisionForUserPermission.CanCreate = permission.CanCreate;
			entitySubdivisionForUserPermission.CanRead = permission.CanRead;
			entitySubdivisionForUserPermission.CanUpdate = permission.CanUpdate;
			entitySubdivisionForUserPermission.CanDelete = permission.CanDelete;

			return entitySubdivisionForUserPermission;
		}
		
		#endregion
	}
}
