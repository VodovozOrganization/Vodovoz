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
using Vodovoz.Domain.Permissions;
using Vodovoz.Repositories.Permissions;

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

		EntitySubdivisionPermissionModel model;

		public void ConfigureDlg(IUnitOfWork uow, Subdivision subdivision)
		{
			UoW = uow;
			this.subdivision = subdivision;
			model = new EntitySubdivisionPermissionModel(UoW, subdivision,new PermissionExtensionStore());

			var columns = ColumnsConfigFactory.Create<PermissionNode>()
				.AddColumn("Документ").AddTextRenderer(x => x.TypeOfEntity.CustomName)
				.AddColumn("Просмотр").AddToggleRenderer(x => x.EntitySubdivisionOnlyPermission.CanRead).Editing()
				.AddColumn("Создание").AddToggleRenderer(x => x.EntitySubdivisionOnlyPermission.CanCreate).Editing()
				.AddColumn("Редактирование").AddToggleRenderer(x => x.EntitySubdivisionOnlyPermission.CanUpdate).Editing()
				.AddColumn("Удаление").AddToggleRenderer(x => x.EntitySubdivisionOnlyPermission.CanDelete).Editing();

			var extensions = model.ExtensionFactory.PermissionExtensions;

			foreach(var item in extensions)
				columns.AddColumn(item.Value.Name).AddToggleRenderer(x => x.EntityPermissionExtended[item.Key].IsPermissionAvailable);

			ytreeviewPermissions.ColumnsConfig = columns.Finish();

			ytreeviewPermissions.ItemsDataSource = model.ObservablePermissionsList;

			ytreeviewEntitiesList.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();

			ytreeviewEntitiesList.ItemsDataSource = model.ObservableTypeOfEntitiesList;

			Sensitive = true;
		}

		private void AddPermission()
		{
			var selected = ytreeviewEntitiesList.GetSelectedObject() as TypeOfEntity;
			model.AddPermission(selected);
		}

		private void OnButtonAddClicked(object sender, EventArgs e)
		{
			AddPermission();
		}

		protected void OnYtreeviewEntitiesListRowActivated(object o, RowActivatedArgs args)
		{
			AddPermission();
		}

		private void DeletePermission()
		{
			var selected = ytreeviewPermissions.GetSelectedObject() as PermissionNode;
			model.DeletePermission(selected);
		}

		private void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			DeletePermission();
		}

		protected void OnYtreeviewPermissionsRowActivated(object o, RowActivatedArgs args)
		{
			DeletePermission();
		}
	}

	internal sealed class EntitySubdivisionPermissionModel
	{
		private IUnitOfWork uow;
		private Subdivision subdivision;
		private IList<PermissionNode> originalPermissionList;
		private IList<TypeOfEntity> originalTypeOfEntityList;
		public IPermissionExtensionFactory ExtensionFactory { get; set; }

		public GenericObservableList<PermissionNode> ObservablePermissionsList { get; private set; }
		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntitySubdivisionPermissionModel(IUnitOfWork uow, Subdivision subdivision, IPermissionExtensionFactory extensionFactory)
		{
			this.subdivision = subdivision;
			this.uow = uow;
			ExtensionFactory = extensionFactory ?? throw new NullReferenceException(nameof(extensionFactory));

			originalPermissionList = PermissionRepository.GetAllSubdivisionEntityPermissions(uow, subdivision.Id, ExtensionFactory).ToList();
			ObservablePermissionsList = new GenericObservableList<PermissionNode>(originalPermissionList);

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
			if(entityNode == null) {
				return;
			}

			ObservableTypeOfEntitiesList.Remove(entityNode);
			PermissionNode savedPermission;
			var foundOriginalPermission = originalPermissionList.FirstOrDefault(x => x.TypeOfEntity == entityNode);
			if(foundOriginalPermission == null) {
				savedPermission = new PermissionNode();
				savedPermission.EntitySubdivisionOnlyPermission = new EntitySubdivisionOnlyPermission {
					Subdivision = subdivision,
					TypeOfEntity = entityNode
				};
				savedPermission.EntityPermissionExtended = new SortedList<string, EntityPermissionExtended>(StringComparer.Ordinal);
				foreach(var item in ExtensionFactory.PermissionExtensions) {
					var node = new EntityPermissionExtended();
					node.Subdivision = subdivision;
					node.TypeOfEntity = entityNode;
					node.PermissionId = item.Value.PermissionId;
					savedPermission.EntityPermissionExtended.Add(node.PermissionId,node);
				}
				savedPermission.TypeOfEntity = entityNode;
				ObservablePermissionsList.Add(savedPermission);
			} else {
				savedPermission = foundOriginalPermission;
				ObservablePermissionsList.Add(savedPermission);
			}
			uow.Save(savedPermission.EntitySubdivisionOnlyPermission);
			foreach(var permission in savedPermission.EntityPermissionExtended)
				uow.Save(permission.Value);
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
				uow.Delete(permission.Value);
		}
	}
}
