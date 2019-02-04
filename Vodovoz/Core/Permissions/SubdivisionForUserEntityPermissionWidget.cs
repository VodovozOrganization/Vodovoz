using System;
using Vodovoz.Representations;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Vodovoz.Repositories.HumanResources;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using Gamma.Binding;
using Gamma.GtkWidgets;
using System.ServiceModel.Configuration;
using Vodovoz.Domain.Permissions;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Repositories.Permissions;
using QS.Project.Repositories;
using System.Linq;
using QSOrmProject.Domain;
using QS.Widgets.Gtk;

namespace Vodovoz.Core.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SubdivisionForUserEntityPermissionWidget : Gtk.Bin, IUserPermissionTab
	{
		public IUnitOfWork UoW { get; set; }
		public string Title => "Особые права на подразделения";

		Subdivision employeeSubdivision;
		EntitySubdivisionForUserPermissionModel model;
		UserBase user;

		public SubdivisionForUserEntityPermissionWidget()
		{
			this.Build();
			Sensitive = false;
		}

		public void ConfigureDlg(IUnitOfWork uow, UserBase user)
		{
			var employee = EmployeeRepository.GetEmployeesForUser(uow, user.Id).FirstOrDefault();
			if(employee == null) {
				MessageDialogHelper.RunWarningDialog($"К пользователю \"{user.Name}\" не привязан сотрудник, невозможно будет установить права на подразделение для документов.");
				return;
			}

			if(employee.Subdivision == null) {
				MessageDialogHelper.RunWarningDialog($"Сотрудник \"{employee.ShortName}\" не привязан к подразделению, невозможно будет установить права на подразделение для документов.");
				return;
			}

			UoW = uow;
			employeeSubdivision = employee.Subdivision;
			this.user = user;

			model = new EntitySubdivisionForUserPermissionModel(UoW, employeeSubdivision, user);

			var subdivisionsVM = new SubdivisionsVM(UoW);
			treeviewSubdivisions.RepresentationModel = subdivisionsVM;
			treeviewSubdivisions.YTreeModel = new RecursiveTreeModel<SubdivisionVMNode>(subdivisionsVM.Result, x => x.Parent, x => x.Children);

			ytreeviewPermissions.ColumnsConfig = ColumnsConfigFactory.Create<EntitySubdivisionForUserPermission>()
				.AddColumn("Подразделение").AddTextRenderer(x => x.Subdivision.Name)
				.AddColumn("Документ").AddTextRenderer(x => x.TypeOfEntity.CustomName)
				.AddColumn("Просмотр").AddToggleRenderer(x => x.CanRead).Editing()
				.AddColumn("Создание").AddToggleRenderer(x => x.CanCreate).Editing()
				.AddColumn("Редактирование").AddToggleRenderer(x => x.CanUpdate).Editing()
				.AddColumn("Удаление").AddToggleRenderer(x => x.CanDelete).Editing()
				.Finish();

			ytreeviewPermissions.ItemsDataSource = model.ObservablePermissionsList;

			ytreeviewEntities.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();

			ytreeviewEntities.ItemsDataSource = model.ObservableTypeOfEntitiesList;

			Sensitive = true;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var subdivisionNode = treeviewSubdivisions.GetSelectedObject() as SubdivisionVMNode;
			if(subdivisionNode == null) {
				return;
			}
			var subdivision = UoW.GetById<Subdivision>(subdivisionNode.Id);

			var typeOfEntity = ytreeviewEntities.GetSelectedObject() as TypeOfEntity;

			if(subdivision == null || typeOfEntity == null) {
				return;
			}
			if(subdivision == employeeSubdivision) {
				MessageDialogHelper.RunWarningDialog("Нельзя добавлять данный вид прав для текущего подразделения сотрудника");
				return;
			}
			if(model.PermissionExists(typeOfEntity, subdivision)) {
				MessageDialogHelper.RunWarningDialog("Такое право уже существует");
				return;
			}
			model.AddPermission(typeOfEntity, subdivision);
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var permission = ytreeviewPermissions.GetSelectedObject() as EntitySubdivisionForUserPermission;
			if(permission == null) {
				return;
			}
			model.DeletePermission(permission);
		}

		public void Save()
		{
			model.Save();
		}
	}

	public class EntitySubdivisionForUserPermissionModel
	{
		private IUnitOfWork uow;
		private Subdivision subdivision;
		private UserBase user;

		private IList<EntitySubdivisionForUserPermission> originalPermissionList;
		private IList<EntitySubdivisionForUserPermission> deletionPermissionList = new List<EntitySubdivisionForUserPermission>();
		private IList<TypeOfEntity> originalTypeOfEntityList;

		public GenericObservableList<EntitySubdivisionForUserPermission> ObservablePermissionsList { get; private set; }
		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntitySubdivisionForUserPermissionModel(IUnitOfWork uow, Subdivision subdivision, UserBase user)
		{
			this.subdivision = subdivision;
			this.uow = uow;
			this.user = user;

			originalPermissionList = PermissionRepository.GetAllSubdivisionForUserEntityPermissions(uow, user.Id);
			ObservablePermissionsList = new GenericObservableList<EntitySubdivisionForUserPermission>(originalPermissionList.ToList());

			originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntity(uow);
			//убираем типы уже загруженные в права
			foreach(var item in originalPermissionList) {
				if(originalTypeOfEntityList.Contains(item.TypeOfEntity)) {
					originalTypeOfEntityList.Remove(item.TypeOfEntity);
				}
			}
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(originalTypeOfEntityList);
		}

		public void AddPermission(TypeOfEntity typeOfEntity, Subdivision subdivision)
		{
			if(typeOfEntity == null || subdivision == null || PermissionExists(typeOfEntity, subdivision)) {
				return;
			}

			EntitySubdivisionForUserPermission savedPermission;
			var foundOriginalPermission = originalPermissionList.FirstOrDefault(x => x.TypeOfEntity == typeOfEntity);
			if(foundOriginalPermission == null) {
				savedPermission = new EntitySubdivisionForUserPermission() {
					Subdivision = subdivision,
					TypeOfEntity = typeOfEntity,
					User = user
				};
				ObservablePermissionsList.Add(savedPermission);
			} else {
				if(deletionPermissionList.Contains(foundOriginalPermission)) {
					deletionPermissionList.Remove(foundOriginalPermission);
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
				deletionPermissionList.Add(deletedPermission);
			}
		}

		public bool PermissionExists(TypeOfEntity type, Subdivision subdivision)
		{
			return ObservablePermissionsList.Any(x => x.TypeOfEntity == type && x.Subdivision == subdivision);
		}

		public void Save()
		{
			foreach(EntitySubdivisionForUserPermission item in ObservablePermissionsList) {
				uow.Save(item);
			}

			foreach(EntitySubdivisionForUserPermission item in deletionPermissionList) {
				uow.Delete(item);
			}
		}
	}
}
