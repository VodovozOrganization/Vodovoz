using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Project.Domain;
using QS.Project.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;
using VodovozBusiness.Services.Subdivisions;

namespace Vodovoz.ViewModels.Permissions
{
	public sealed class EntitySubdivisionPermissionViewModel
	{
		private readonly IUnitOfWork _uow;
		private readonly IList<SubdivisionPermissionNode> _originalPermissionList;
		private readonly List<TypeOfEntity> _originalTypeOfEntityList;

		public EntitySubdivisionPermissionViewModel(
			IUnitOfWork uow,
			Subdivision subdivision,
			PermissionListViewModel permissionListViewModel,
			IPermissionRepository permissionRepository)
		{
			if(permissionRepository is null)
			{
				throw new ArgumentNullException(nameof(permissionRepository));
			}

			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			Subdivision = subdivision ?? throw new ArgumentNullException(nameof(subdivision));
			PermissionListViewModel = permissionListViewModel ?? throw new NullReferenceException(nameof(permissionListViewModel));

			ExtensionStore = PermissionListViewModel.PermissionExtensionStore;
			var permissionList = permissionRepository.GetAllSubdivisionEntityPermissions(uow, subdivision.Id, ExtensionStore);
			_originalPermissionList = permissionList.ToList();

			PermissionListViewModel.PermissionsList = new ObservableList<IPermissionNode>(permissionList.OfType<IPermissionNode>().ToList());
			PermissionListViewModel.PermissionsList.CollectionChanged += PermissionsListCollectionChanged;

			_originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntityOrderedByName(uow).ToList();
			//убираем типы уже загруженные в права
			foreach(var item in _originalPermissionList)
			{
				if(_originalTypeOfEntityList.Contains(item.TypeOfEntity))
				{
					_originalTypeOfEntityList.Remove(item.TypeOfEntity);
				}
			}

			SortTypeOfEntityList();
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(_originalTypeOfEntityList);
		}

		public IPermissionExtensionStore ExtensionStore { get; set; }
		public PermissionListViewModel PermissionListViewModel { get; set; }
		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		private void PermissionsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(e.Action != NotifyCollectionChangedAction.Remove)
			{
				return;
			}

			foreach(var item in e.OldItems)
			{
				if(item is SubdivisionPermissionNode node)
				{
					AddTypeToTypeOfEntitiesList(node.TypeOfEntity);
					DeletePermission(node);
				}
			}
		}

		public Subdivision Subdivision { get; }

		public void AddPermission(TypeOfEntity entityNode)
		{
			if(entityNode == null)
			{
				return;
			}

			ObservableTypeOfEntitiesList.Remove(entityNode);
			SubdivisionPermissionNode savedPermission;
			var foundOriginalPermission = _originalPermissionList.FirstOrDefault(x => x.TypeOfEntity == entityNode);
			if(foundOriginalPermission == null)
			{
				savedPermission = new SubdivisionPermissionNode();
				savedPermission.EntitySubdivisionOnlyPermission = new EntitySubdivisionOnlyPermission
				{
					Subdivision = Subdivision,
					TypeOfEntity = entityNode
				};
				savedPermission.EntityPermissionExtended = new List<EntitySubdivisionPermissionExtended>();
				foreach(var item in ExtensionStore.PermissionExtensions)
				{
					var node = new EntitySubdivisionPermissionExtended();
					node.Subdivision = Subdivision;
					node.TypeOfEntity = entityNode;
					node.PermissionId = item.PermissionId;
					savedPermission.EntityPermissionExtended.Add(node);
				}
				savedPermission.TypeOfEntity = entityNode;
				PermissionListViewModel.PermissionsList.Add(savedPermission);
			}
			else
			{
				savedPermission = foundOriginalPermission;
				PermissionListViewModel.PermissionsList.Add(savedPermission);
			}
		}

		public void AddPermissionsFromSubdivision(ISubdivisionPermissionsService subdivisionPermissionsService, Subdivision sourceSubdivision)
		{
			var newPermissions = subdivisionPermissionsService.AddSubdivisionEntityPermissions(
				_uow,
				Subdivision,
				sourceSubdivision);

			RepalcePermissions(newPermissions);
		}

		public void ReplacePermissionsFromSubdivision(ISubdivisionPermissionsService subdivisionPermissionsService, Subdivision sourceSubdivision)
		{
			var newPermissions = subdivisionPermissionsService.ReplaceSubdivisionEntityPermissions(
				_uow,
				Subdivision,
				sourceSubdivision);

			RepalcePermissions(newPermissions);
		}

		private void RepalcePermissions(IEnumerable<SubdivisionPermissionNode> permissions)
		{
			if(permissions == null || permissions.Count() == 0)
			{
				return;
			}

			while(PermissionListViewModel.PermissionsList.Any())
			{
				PermissionListViewModel.PermissionsList.RemoveAt(0);
			}

			foreach(var permission in permissions)
			{
				AddPermission(permission);
			}
		}

		private void AddPermission(SubdivisionPermissionNode permission)
		{
			if(permission == null)
			{
				return;
			}

			RemoveTypeFromTypeOfEntitiesList(permission.TypeOfEntity);

			PermissionListViewModel.PermissionsList.Add(permission);
		}

		public void AddTypeToTypeOfEntitiesList(TypeOfEntity typeOfEntity)
		{
			if(typeOfEntity == null)
			{
				return;
			}

			var isTypeOfEntityAlreadyInList =
				ObservableTypeOfEntitiesList
				.Any(x => x.Type == typeOfEntity.Type);

			if(isTypeOfEntityAlreadyInList)
			{
				return;
			}

			ObservableTypeOfEntitiesList.Add(typeOfEntity);

			SortTypeOfEntityList();
		}

		public void RemoveTypeFromTypeOfEntitiesList(TypeOfEntity typeOfEntity)
		{
			if(typeOfEntity == null)
			{
				return;
			}

			var itemToRemove =
				ObservableTypeOfEntitiesList.Where(x => x.Type == typeOfEntity.Type)
				.FirstOrDefault();

			if(itemToRemove is null)
			{
				return;
			}

			ObservableTypeOfEntitiesList.Remove(itemToRemove);
			SortTypeOfEntityList();
		}

		private void DeletePermission(SubdivisionPermissionNode deletedPermission)
		{
			if(deletedPermission == null)
			{
				return;
			}

			_uow.Delete(deletedPermission.EntitySubdivisionOnlyPermission);

			foreach(var permission in deletedPermission.EntityPermissionExtended)
			{
				_uow.Delete(permission);
			}
		}

		public void SavePermissions()
		{
			IEnumerable<EntitySubdivisionOnlyPermission> permissionList = PermissionListViewModel.PermissionsList
					.Select(x => x.EntityPermission)
					.OfType<EntitySubdivisionOnlyPermission>();

			foreach(var item in permissionList)
			{
				_uow.Save(item);
			}

			PermissionListViewModel.SaveExtendedPermissions(_uow);
		}

		private void SortTypeOfEntityList()
		{
			if(_originalTypeOfEntityList?.FirstOrDefault() == null)
			{
				return;
			}

			_originalTypeOfEntityList.Sort((x, y) =>
					string.Compare(x.CustomName ?? x.Type, y.CustomName ?? y.Type));
		}
	}
}
