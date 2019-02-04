using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.UoW;
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
			model = new EntitySubdivisionPermissionModel(UoW, subdivision);

			ytreeviewPermissions.ColumnsConfig = ColumnsConfigFactory.Create<EntitySubdivisionOnlyPermission>()
				.AddColumn("Документ").AddTextRenderer(x => x.TypeOfEntity.CustomName)
				.AddColumn("Просмотр").AddToggleRenderer(x => x.CanRead).Editing()
				.AddColumn("Создание").AddToggleRenderer(x => x.CanCreate).Editing()
				.AddColumn("Редактирование").AddToggleRenderer(x => x.CanUpdate).Editing()
				.AddColumn("Удаление").AddToggleRenderer(x => x.CanDelete).Editing()
				.Finish();

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
			var selected = ytreeviewPermissions.GetSelectedObject() as EntitySubdivisionOnlyPermission;
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
		private IList<EntitySubdivisionOnlyPermission> originalPermissionList;
		private IList<TypeOfEntity> originalTypeOfEntityList;

		public GenericObservableList<EntitySubdivisionOnlyPermission> ObservablePermissionsList { get; private set; }
		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntitySubdivisionPermissionModel(IUnitOfWork uow, Subdivision subdivision)
		{
			this.subdivision = subdivision;
			this.uow = uow;

			originalPermissionList = PermissionRepository.GetAllSubdivisionEntityPermissions(uow, subdivision.Id);
			ObservablePermissionsList = new GenericObservableList<EntitySubdivisionOnlyPermission>(originalPermissionList.ToList());

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
			EntitySubdivisionOnlyPermission savedPermission;
			var foundOriginalPermission = originalPermissionList.FirstOrDefault(x => x.TypeOfEntity == entityNode);
			if(foundOriginalPermission == null) {
				savedPermission = new EntitySubdivisionOnlyPermission() {
					Subdivision = subdivision,
					TypeOfEntity = entityNode
				};
				ObservablePermissionsList.Add(savedPermission);
			} else {
				savedPermission = foundOriginalPermission;
				ObservablePermissionsList.Add(savedPermission);
			}
			uow.Save(savedPermission);
		}

		public void DeletePermission(EntitySubdivisionOnlyPermission deletedPermission)
		{
			if(deletedPermission == null) {
				return;
			}
			ObservableTypeOfEntitiesList.Add(deletedPermission.TypeOfEntity);
			ObservablePermissionsList.Remove(deletedPermission);
			uow.Delete(deletedPermission);
		}
	}
}
