using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Project.Services.GtkUI;
using Vodovoz.Domain.Permissions;
using Vodovoz.PermissionExtensions;
using Vodovoz.Repositories.Permissions;
using Vodovoz.ViewModels;

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
			var permissionExtension = PermissionExtensionSingletonStore.GetInstance();
			permissionlistview.ViewModel = new PermissionListViewModel(new GtkInteractiveService(), permissionExtension);
			ViewModel = new EntitySubdivisionPermissionModel(UoW, subdivision, permissionExtension, permissionlistview.ViewModel);

			var extensions = ViewModel.ExtensionStore.PermissionExtensions;

			extensions.OrderBy(x => x.PermissionId);

			foreach(PermissionNode item in ViewModel.ObservablePermissionsList)
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
		private IList<PermissionNode> originalPermissionList;
		private IList<TypeOfEntity> originalTypeOfEntityList;
		public PermissionExtensionSingletonStore ExtensionStore { get; set; }
		public PermissionListViewModel PermissionListViewModel { get; set; }

		public GenericObservableList<PermissionNode> ObservablePermissionsList { get; private set; }
		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntitySubdivisionPermissionModel(IUnitOfWork uow, Subdivision subdivision, PermissionExtensionSingletonStore extensionStore, PermissionListViewModel permissionListViewModel)
		{
			ExtensionStore = extensionStore ?? throw new NullReferenceException(nameof(extensionStore));
			PermissionListViewModel = permissionListViewModel ?? throw new NullReferenceException(nameof(permissionListViewModel));

			this.subdivision = subdivision;
			this.uow = uow;

			originalPermissionList = PermissionRepository.GetAllSubdivisionEntityPermissions(uow, subdivision.Id, ExtensionStore).ToList();
			ObservablePermissionsList = new GenericObservableList<PermissionNode>(originalPermissionList);
			ObservablePermissionsList.ElementRemoved += (aList, aIdx, aObject) => DeletePermission(aObject as PermissionNode);

			PermissionListViewModel.PermissionsList = ObservablePermissionsList;

			originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntity(uow);
			//убираем типы уже загруженные в права
			foreach(var item in originalPermissionList) {
				if(originalTypeOfEntityList.Contains(item.TypeOfEntity)) {
					originalTypeOfEntityList.Remove(item.TypeOfEntity);
				}
			}
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(originalTypeOfEntityList);
		}

		public void AddPermission(TypeOfEntity entityNode)
		{
			if(entityNode == null) 
				return;

			ObservableTypeOfEntitiesList.Remove(entityNode);
			PermissionNode savedPermission;
			var foundOriginalPermission = originalPermissionList.FirstOrDefault(x => x.TypeOfEntity == entityNode);
			if(foundOriginalPermission == null) {
				savedPermission = new PermissionNode();
				savedPermission.EntitySubdivisionOnlyPermission = new EntitySubdivisionOnlyPermission {
					Subdivision = subdivision,
					TypeOfEntity = entityNode
				};
				savedPermission.EntityPermissionExtended = new List<EntityPermissionExtended>();
				foreach(var item in ExtensionStore.PermissionExtensions) {
					var node = new EntityPermissionExtended();
					node.Subdivision = subdivision;
					node.TypeOfEntity = entityNode;
					node.PermissionId = item.PermissionId;
					savedPermission.EntityPermissionExtended.Add(node);
				}
				savedPermission.TypeOfEntity = entityNode;
				ObservablePermissionsList.Add(savedPermission);
			} else {
				savedPermission = foundOriginalPermission;
				ObservablePermissionsList.Add(savedPermission);
			}
			uow.Save(savedPermission.EntitySubdivisionOnlyPermission);
		}

		public void DeletePermission(PermissionNode deletedPermission)
		{
			if(deletedPermission == null) {
				return;
			}
			ObservableTypeOfEntitiesList.Add(deletedPermission.TypeOfEntity);
			ObservablePermissionsList.Remove(deletedPermission);
			uow.Delete(deletedPermission.EntitySubdivisionOnlyPermission);
			foreach(var permission in deletedPermission.EntityPermissionExtended)
				uow.Delete(permission);
		}
	}
}
