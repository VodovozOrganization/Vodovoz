using Autofac;
using Gamma.Binding;
using Gamma.GtkWidgets;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Widgets.GtkUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Organizations;

namespace Vodovoz.Core.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SubdivisionForUserEntityPermissionWidget : Gtk.Bin, IUserPermissionTab
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private IGuiDispatcher _guiDispatcher;
		private IEmployeeRepository _employeeRepository;
		private SubdivisionsJournalViewModel _subdivisionJVM;

		public string Title => "Особые права на подразделения";

		Subdivision employeeSubdivision;
		
		public SubdivisionForUserEntityPermissionWidget()
		{
			ResolveDependencies();
			Build();
			Sensitive = false;
		}

		private void ResolveDependencies()
		{
			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_guiDispatcher = _lifetimeScope.Resolve<IGuiDispatcher>();
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
				.AddColumn("Название").AddTextRenderer(node => node.Name).AddSetter((cell, node) =>
				{
					var color = GdkColors.PrimaryText;
					if(node.IsArchive)
					{
						color = GdkColors.InsensitiveText;
					}

					cell.ForegroundGdk = color;
				})
				.AddColumn("Руководитель").AddTextRenderer(node => node.ChiefName).AddSetter((cell, node) =>
				{
					var color = GdkColors.PrimaryText;
					if(node.IsArchive)
					{
						color = GdkColors.InsensitiveText;
					}

					cell.ForegroundGdk = color;
				})
				.AddColumn("Код").AddNumericRenderer(node => node.Id).AddSetter((cell, node) =>
				{
					var color = GdkColors.PrimaryText;
					if(node.IsArchive)
					{
						color = GdkColors.InsensitiveText;
					}

					cell.ForegroundGdk = color;
				})
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
			_guiDispatcher.RunInGuiTread(() =>
			{
				treeviewSubdivisions.CollapseAll();
				treeviewSubdivisions.YTreeModel = new RecursiveTreeModel<SubdivisionJournalNode>(_subdivisionJVM.Items.Cast<SubdivisionJournalNode>(), _subdivisionJVM.RecuresiveConfig);
				treeviewSubdivisions.ExpandAll();
			});
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

		public override void Destroy()
		{
			_employeeRepository = null;
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
