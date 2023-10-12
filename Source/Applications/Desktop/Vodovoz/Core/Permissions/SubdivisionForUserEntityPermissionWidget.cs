using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core.Lifetime;
using FluentNHibernate.Conventions;
using Gamma.Binding;
using Gamma.GtkWidgets;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Widgets.GtkUI;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Representations;

namespace Vodovoz.Core.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SubdivisionForUserEntityPermissionWidget : Gtk.Bin, IUserPermissionTab
	{
		private readonly ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private SubdivisionsJournalViewModel _subdivisionJVM;

		public string Title => "Особые права на подразделения";

		Subdivision employeeSubdivision;
		
		public SubdivisionForUserEntityPermissionWidget()
		{
			Build();
			Sensitive = false;
		}

		public void ConfigureDlg(IUnitOfWork uow, UserBase user)
		{
			var employee = _employeeRepository.GetEmployeesForUser(uow, user.Id).FirstOrDefault();
			if(employee == null) {
				MessageDialogHelper.RunWarningDialog($"К пользователю \"{user.Name}\" не привязан сотрудник, невозможно будет установить права на подразделение для документов.");
				return;
			}

			if(employee.Subdivision == null) {
				MessageDialogHelper.RunWarningDialog($"Сотрудник \"{employee.ShortName}\" не привязан к подразделению, невозможно будет установить права на подразделение для документов.");
				return;
			}

			ViewModel = new EntitySubdivisionForUserPermissionViewModel(uow, user);

			_subdivisionJVM = _lifetimeScope.Resolve<SubdivisionsJournalViewModel>();

			_subdivisionJVM.Refresh();
			
			treeviewSubdivisions.CreateFluentColumnsConfig<SubdivisionJournalNode>()
				.AddColumn("Подразделение").AddTextRenderer(x => x.Name)
				.Finish();

			_subdivisionJVM.DataLoader.ItemsListUpdated += SubdivisionsListReloaded;

			ytreeviewPermissions.ColumnsConfig = ColumnsConfigFactory.Create<EntitySubdivisionForUserPermission>()
				.AddColumn("Подразделение").AddTextRenderer(x => x.Subdivision.Name)
				.AddColumn("Документ").AddTextRenderer(x => x.TypeOfEntity.CustomName)
				.AddColumn("Просмотр").AddToggleRenderer(x => x.CanRead).Editing()
				.AddColumn("Создание").AddToggleRenderer(x => x.CanCreate).Editing()
				.AddColumn("Редактирование").AddToggleRenderer(x => x.CanUpdate).Editing()
				.AddColumn("Удаление").AddToggleRenderer(x => x.CanDelete).Editing()
				.Finish();

			ytreeviewPermissions.ItemsDataSource = ViewModel.ObservablePermissionsList;

			ytreeviewEntities.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();

			ytreeviewEntities.ItemsDataSource = ViewModel.ObservableTypeOfEntitiesList;
			
			searchSubdivisions.TextChanged += SearchSubdivisionsOnTextChanged;
			searchTypesOfEntities.TextChanged += SearchPermissionsOnTextChanged;

			treeviewSubdivisions.ExpandAll();
			
			Sensitive = true;
		}

		private void SubdivisionsListReloaded(object sender, EventArgs e)
		{
			treeviewSubdivisions.CollapseAll();
			treeviewSubdivisions.YTreeModel = new RecursiveTreeModel<SubdivisionJournalNode>(_subdivisionJVM.Items.Cast<SubdivisionJournalNode>(), _subdivisionJVM.RecuresiveConfig);
			treeviewSubdivisions.ExpandAll();
		}

		public EntitySubdivisionForUserPermissionViewModel ViewModel { get; set; }

		private void SearchSubdivisionsOnTextChanged(object sender, EventArgs e)
		{
			_subdivisionJVM.Search.SearchValues = searchSubdivisions.Text.ToLower().Split(' ');
		}
		
		private void SearchPermissionsOnTextChanged(object sender, EventArgs e)
		{
			ytreeviewEntities.ItemsDataSource = null;
			ViewModel.SearchPermissions(searchTypesOfEntities.Text);
			ytreeviewEntities.ItemsDataSource = ViewModel.ObservableTypeOfEntitiesList;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var subdivisionNode = treeviewSubdivisions.GetSelectedObject() as SubdivisionJournalNode;
			if(subdivisionNode == null) {
				return;
			}
			var subdivision = ViewModel.Uow.GetById<Subdivision>(subdivisionNode.Id);

			var typeOfEntity = ytreeviewEntities.GetSelectedObject() as TypeOfEntity;

			if(subdivision == null || typeOfEntity == null) {
				return;
			}
			if(subdivision == employeeSubdivision) {
				MessageDialogHelper.RunWarningDialog("Нельзя добавлять данный вид прав для текущего подразделения сотрудника");
				return;
			}
			if(ViewModel.PermissionExists(typeOfEntity, subdivision)) {
				MessageDialogHelper.RunWarningDialog("Такое право уже существует");
				return;
			}
			ViewModel.AddPermission(typeOfEntity, subdivision);
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var permission = ytreeviewPermissions.GetSelectedObject() as EntitySubdivisionForUserPermission;
			if(permission == null) {
				return;
			}
			ViewModel.DeletePermission(permission);
		}

		public void Save()
		{
			if(ViewModel == null) {
				return;
			}
			ViewModel.Save();
		}

		public void UpdateData(IList<EntitySubdivisionForUserPermission> newEntitySubdivisionForUserPermissions)
		{
			ViewModel.UpdateData(newEntitySubdivisionForUserPermissions);
			ytreeviewPermissions.ItemsDataSource = ViewModel.ObservablePermissionsList;
			ytreeviewEntities.ItemsDataSource = ViewModel.ObservableTypeOfEntitiesList;
		}
	}

	public class EntitySubdivisionForUserPermissionViewModel
	{
		private readonly IPermissionRepository _permissionRepository = new PermissionRepository();
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

			_originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntity(uow);
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
			_originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntity(Uow);
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
