using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Core.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SubdivisionEntityPermissionWidget : Bin
	{
		public IUnitOfWork UoW { get; set; }

		Subdivision subdivision;

		public SubdivisionEntityPermissionWidget()
		{
			this.Build();
			Sensitive = false;
		}

		internal virtual EntitySubdivisionPermissionModel ViewModel { get; set; }

		public void ConfigureDlg(IUnitOfWork uow, Subdivision subdivision)
		{
			UoW = uow;
			this.subdivision = subdivision;
			var permissionExtensionStore = PermissionExtensionSingletonStore.GetInstance();
			permissionlistview.ViewModel = new PermissionListViewModel(permissionExtensionStore);
			ViewModel = new EntitySubdivisionPermissionModel(UoW, subdivision, permissionlistview.ViewModel, new EntityRepositories.Permissions.PermissionRepository());

			var extensions = ViewModel.ExtensionStore.PermissionExtensions;

			extensions.OrderBy(x => x.PermissionId);

			foreach(SubdivisionPermissionNode item in ViewModel.PermissionListViewModel.PermissionsList)
				item.EntityPermissionExtended.OrderBy(x => x.PermissionId);

			ytreeviewEntitiesList.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();

			ytreeviewEntitiesList.ItemsDataSource = ViewModel.ObservableTypeOfEntitiesList;

			Sensitive = true;
		}

		private void AddPermission()
		{
			var selected = ytreeviewEntitiesList.GetSelectedObject() as TypeOfEntity;
			ViewModel.AddPermission(selected);
		}

		private void OnButtonAddClicked(object sender, EventArgs e)
		{
			AddPermission();
		}

		protected void OnYtreeviewEntitiesListRowActivated(object o, RowActivatedArgs args)
		{
			AddPermission();
		}
	}

	internal sealed class EntitySubdivisionPermissionModel
	{
		private IUnitOfWork uow;
		private Subdivision subdivision;
		private IList<SubdivisionPermissionNode> originalPermissionList;
		private List<TypeOfEntity> originalTypeOfEntityList;
		public IPermissionExtensionStore ExtensionStore { get; set; }
		public PermissionListViewModel PermissionListViewModel { get; set; }

		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntitySubdivisionPermissionModel(IUnitOfWork uow, Subdivision subdivision, PermissionListViewModel permissionListViewModel, IPermissionRepository permissionRepository)
		{
			PermissionListViewModel = permissionListViewModel ?? throw new NullReferenceException(nameof(permissionListViewModel));

			this.subdivision = subdivision;
			this.uow = uow;
			ExtensionStore = PermissionListViewModel.PermissionExtensionStore;
			var permissionList = permissionRepository.GetAllSubdivisionEntityPermissions(uow, subdivision.Id, ExtensionStore);
			originalPermissionList = permissionList.ToList(); 

			PermissionListViewModel.PermissionsList = new GenericObservableList<IPermissionNode>(permissionList.OfType<IPermissionNode>().ToList());
			PermissionListViewModel.PermissionsList.ElementRemoved += (aList, aIdx, aObject) => DeletePermission(aObject as SubdivisionPermissionNode);

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

		public void AddPermission(TypeOfEntity entityNode)
		{
			if(entityNode == null) 
				return;

			ObservableTypeOfEntitiesList.Remove(entityNode);
			SubdivisionPermissionNode savedPermission;
			var foundOriginalPermission = originalPermissionList.FirstOrDefault(x => x.TypeOfEntity == entityNode);
			if(foundOriginalPermission == null) {
				savedPermission = new SubdivisionPermissionNode();
				savedPermission.EntitySubdivisionOnlyPermission = new EntitySubdivisionOnlyPermission {
					Subdivision = subdivision,
					TypeOfEntity = entityNode
				};
				savedPermission.EntityPermissionExtended = new List<EntitySubdivisionPermissionExtended>();
				foreach(var item in ExtensionStore.PermissionExtensions) {
					var node = new EntitySubdivisionPermissionExtended();
					node.Subdivision = subdivision;
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
			uow.Delete(deletedPermission.EntitySubdivisionOnlyPermission);
			foreach(var permission in deletedPermission.EntityPermissionExtended)
				uow.Delete(permission);
			SortTypeOfEntityList();
		}

		public void SavePermissions(IUnitOfWork uow)
		{
			IEnumerable<EntitySubdivisionOnlyPermission> permissionList = PermissionListViewModel.PermissionsList
					.Select(x => x.EntityPermission)
					.OfType<EntitySubdivisionOnlyPermission>();

			foreach(var item in permissionList)
				uow.Save(item);

			PermissionListViewModel.SaveExtendedPermissions(uow);
		}

		private void SortTypeOfEntityList()
		{
			if(originalTypeOfEntityList?.FirstOrDefault() == null)
				return;

			originalTypeOfEntityList.Sort((x, y) =>
					string.Compare(x.CustomName ?? x.Type, y.CustomName ?? y.Type));
		}
	}
}
