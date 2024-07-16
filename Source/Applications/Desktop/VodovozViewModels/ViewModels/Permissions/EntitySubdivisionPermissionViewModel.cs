using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Project.Domain;
using QS.Project.Repositories;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.ViewModels.Permissions
{
	public sealed class EntitySubdivisionPermissionViewModel
	{
		private readonly IUnitOfWork _uow;
		private IList<SubdivisionPermissionNode> originalPermissionList;
		private List<TypeOfEntity> originalTypeOfEntityList;
		public IPermissionExtensionStore ExtensionStore { get; set; }
		public PermissionListViewModel PermissionListViewModel { get; set; }

		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntitySubdivisionPermissionViewModel(IUnitOfWork uow, Subdivision subdivision, PermissionListViewModel permissionListViewModel, IPermissionRepository permissionRepository)
		{
			PermissionListViewModel = permissionListViewModel ?? throw new NullReferenceException(nameof(permissionListViewModel));

			Subdivision = subdivision;
			_uow = uow;
			ExtensionStore = PermissionListViewModel.PermissionExtensionStore;
			var permissionList = permissionRepository.GetAllSubdivisionEntityPermissions(uow, subdivision.Id, ExtensionStore);
			originalPermissionList = permissionList.ToList(); 

			PermissionListViewModel.PermissionsList = new ObservableList<IPermissionNode>(permissionList.OfType<IPermissionNode>().ToList());
			PermissionListViewModel.PermissionsList.CollectionChanged += PermissionsListCollectionChanged;

			originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntity(uow).ToList();
			//убираем типы уже загруженные в права
			foreach(var item in originalPermissionList) {
				if(originalTypeOfEntityList.Contains(item.TypeOfEntity)) {
					originalTypeOfEntityList.Remove(item.TypeOfEntity);
				}
			}
			SortTypeOfEntityList();
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(originalTypeOfEntityList);
		}

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
			var foundOriginalPermission = originalPermissionList.FirstOrDefault(x => x.TypeOfEntity == entityNode);
			if(foundOriginalPermission == null) {
				savedPermission = new SubdivisionPermissionNode();
				savedPermission.EntitySubdivisionOnlyPermission = new EntitySubdivisionOnlyPermission {
					Subdivision = Subdivision,
					TypeOfEntity = entityNode
				};
				savedPermission.EntityPermissionExtended = new List<EntitySubdivisionPermissionExtended>();
				foreach(var item in ExtensionStore.PermissionExtensions) {
					var node = new EntitySubdivisionPermissionExtended();
					node.Subdivision = Subdivision;
					node.TypeOfEntity = entityNode;
					node.PermissionId = item.PermissionId;
					savedPermission.EntityPermissionExtended.Add(node);
				}
				savedPermission.TypeOfEntity = entityNode;
				PermissionListViewModel.PermissionsList.Add(savedPermission);
			} else {
				savedPermission = foundOriginalPermission;
				PermissionListViewModel.PermissionsList.Add(savedPermission);
			}
		}

		public void DeletePermission(SubdivisionPermissionNode deletedPermission)
		{
			if(deletedPermission == null) {
				return;
			}
			ObservableTypeOfEntitiesList.Add(deletedPermission.TypeOfEntity);
			PermissionListViewModel.PermissionsList.Remove(deletedPermission);
			_uow.Delete(deletedPermission.EntitySubdivisionOnlyPermission);
			foreach(var permission in deletedPermission.EntityPermissionExtended)
			{
				_uow.Delete(permission);
			}

			SortTypeOfEntityList();
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
			if(originalTypeOfEntityList?.FirstOrDefault() == null)
			{
				return;
			}

			originalTypeOfEntityList.Sort((x, y) =>
					string.Compare(x.CustomName ?? x.Type, y.CustomName ?? y.Type));
		}
	}
}
