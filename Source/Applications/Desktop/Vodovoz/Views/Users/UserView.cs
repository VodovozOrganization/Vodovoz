using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.ViewModels;
using QS.Widgets.GtkUI;
using Vodovoz.Core.Permissions;
using Vodovoz.Core;
using Gtk;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using Vodovoz.Domain.Permissions;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Views.Users
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserView : TabViewBase<UserViewModel>
	{
		private ViewModelWidgetResolver _widgetResolver = ViewModelWidgetResolver.Instance;
		private UserEntityPermissionWidget _documentPermissionWidget;
		private SubdivisionForUserEntityPermissionWidget _specialDocumentPermissionWidget;
		private Widget _warehousePermissionsView;

		public UserView(UserViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDialog();
		}

		private void ConfigureDialog()
		{
			btnAddPermissionsToUser.Clicked += (sender, args) => ViewModel.AddPermissionsToUserCommand.Execute();
			btnChangePermissionsFromUser.Clicked += (sender, args) => ViewModel.ChangePermissionsFromUserCommand.Execute();
			
			_documentPermissionWidget = new UserEntityPermissionWidget();
			_specialDocumentPermissionWidget = new SubdivisionForUserEntityPermissionWidget();

			//Отключены, так как это простейший вариант диалога, вводимый из-за срочности ввода новых прав на склады
			ybuttonSetNewPassword.Visible = false;
			ybuttonResetPassword.Visible = false;
			PasswordWarning.Visible = false;
			ycheckRequirePasswordChange.Visible = false;
			
			buttonUserInfo.Active = true;
			buttonUserInfo.Toggled += (s, e) => notebook.CurrentPage = 0;

			ybuttonPresetPrivileges.Active = false;
			ybuttonPresetPrivileges.Toggled += (s, e) => notebook.CurrentPage = 1;

			ybuttonWarehousePrivileges.Active = false;
			ybuttonWarehousePrivileges.Toggled += (s, e) => notebook.CurrentPage = 2;

			ybuttonDocumentPrivileges.Active = false;
			ybuttonDocumentPrivileges.Toggled += (s, e) => notebook.CurrentPage = 3;

			ybuttonSpecialDocumentPrivileges.Active = false;
			ybuttonSpecialDocumentPrivileges.Toggled += (s, e) => notebook.CurrentPage = 4;

			notebook.ShowTabs = false;

			ytextviewComment.WrapMode = WrapMode.Word;

			ycheckIsAdmin.Binding.AddBinding(ViewModel.Entity, e => e.IsAdmin, w => w.Active).InitializeFromSource();
			ycheckUserDisabled.Binding.AddBinding(ViewModel.Entity, e => e.Deactivated, w => w.Active).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Description, w => w.Buffer.Text).InitializeFromSource();
			ylabelIdValue.Binding.AddFuncBinding(ViewModel.Entity, e => e.Id.ToString(), w => w.LabelProp).InitializeFromSource();
			yentryDisplayName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryLogin.Binding
				.AddBinding(ViewModel.Entity, e => e.Login, w => w.Text)
				.AddBinding(ViewModel, e => e.CanEditLogin, w => w.IsEditable)
				.InitializeFromSource();

			buttonSave.Clicked += (sender, e) => {
				_documentPermissionWidget.Save();
				_specialDocumentPermissionWidget.Save();
				ViewModel.SaveCommand.Execute();
			};

			buttonCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();
			
			var presetPermissionWidget = _widgetResolver.Resolve(ViewModel.PresetPermissionsViewModel);
			vboxPresetPrivileges.Add(presetPermissionWidget);
			presetPermissionWidget.Show();

			CreateWarehousePermissionsView();

			ybuttonDocumentPrivileges.Sensitive = false;
			ybuttonSpecialDocumentPrivileges.Sensitive = false;

			if(ViewModel.Entity.Id != 0)
			{
				_documentPermissionWidget.ConfigureDlg(ViewModel.UoW, ViewModel.Entity);
				vboxDocumentPrivileges.Add(_documentPermissionWidget);
				_documentPermissionWidget.Show();
				ybuttonDocumentPrivileges.Sensitive = true;
				_documentPermissionWidget.Model.PermissionListViewModel.ExportAction = ViewModel.ExportPermissions;
				_documentPermissionWidget.Model.PermissionListViewModel.PermissionsList.ContentChanged += ViewModel.UpdateChanges;

				_specialDocumentPermissionWidget.ConfigureDlg(ViewModel.UoW, ViewModel.Entity);
				vboxSpecialDocumentPrivileges.Add(_specialDocumentPermissionWidget);
				_specialDocumentPermissionWidget.Show();
				ybuttonSpecialDocumentPrivileges.Sensitive = true;
				if(_specialDocumentPermissionWidget.ViewModel != null)
				{
					_specialDocumentPermissionWidget.ViewModel.ObservablePermissionsList.ListContentChanged += ViewModel.UpdateChanges;
				}
			}
			
			ViewModel.UpdateEntityUserPermissionsAction += OnUpdateEntityUserPermissionsAction;
			ViewModel.UpdateEntitySubdivisionForUserPermissionsAction += OnUpdateEntitySubdivisionForUserPermissionsAction;
			ViewModel.UpdateWarehousePermissionsAction += OnUpdateWarehousePermissionsViewAction;

			#region Роли пользователя

			ConfigureTreeViews();
			
			ytextviewRoleDescription.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.UserRoleDescription, w => w.Buffer.Text)
				.AddBinding(vm => vm.HasCurrentUserRole, w => w.Sensitive)
				.InitializeFromSource();
			
			ycomboboxDefaultRole.SetRenderTextFunc<UserRole>(ur => ur.Name);
			ycomboboxDefaultRole.ShowSpecialStateNot = true;
			ycomboboxDefaultRole.ItemsList = ViewModel.Entity.UserRoles;
			ycomboboxDefaultRole.Binding
				.AddBinding(ViewModel.Entity, e => e.CurrentUserRole, w => w.SelectedItem)
				.InitializeFromSource();
			ViewModel.UpdateUserRolesForCurrentRoleAction += ViewModelOnUpdateUserRolesForCurrentRoleAction;
			
			buttonAddRole.Clicked += (sender, args) => ViewModel.AddUserRoleToUserCommand.Execute();
			ViewModel.AddUserRoleToUserCommand.CanExecuteChanged += AddUserRoleToUserCommandOnCanExecuteChanged;
			buttonRemoveRole.Clicked += (sender, args) => ViewModel.RemoveUserRoleCommand.Execute();
			ViewModel.RemoveUserRoleCommand.CanExecuteChanged += RemoveUserRoleCommandOnCanExecuteChanged;

			#endregion
		}

		private void ConfigureTreeViews()
		{
			ytreeviewAvailableRoles.ColumnsConfig = FluentColumnsConfig<UserRole>.Create()
				.AddColumn("Роль пользователя")
					.AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewAvailableRoles.Binding
				.AddBinding(ViewModel, vm => vm.SelectedAvailableUserRole, w => w.SelectedRow)
				.InitializeFromSource();
			ytreeviewAvailableRoles.RowActivated += (o, args) => ViewModel.AddUserRoleToUserCommand.Execute();
			ytreeviewAvailableRoles.ItemsDataSource = ViewModel.AvailableUserRoles;
			
			ytreeviewAddedRoles.ColumnsConfig = FluentColumnsConfig<UserRole>.Create()
				.AddColumn("Роль пользователя")
					.AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewAddedRoles.Binding
				.AddBinding(ViewModel, vm => vm.SelectedUserRole, w => w.SelectedRow)
				.InitializeFromSource();
			ytreeviewAddedRoles.RowActivated += (o, args) => ViewModel.RemoveUserRoleCommand.Execute();
			ytreeviewAddedRoles.ItemsDataSource = ViewModel.Entity.UserRoles;
			ytreeviewAvailableRoles.Sensitive = ytreeviewAddedRoles.Sensitive = !ViewModel.IsSameUser && ViewModel.HasUserOnServer;
		}

		private void OnUpdateEntitySubdivisionForUserPermissionsAction(IList<EntitySubdivisionForUserPermission> newUserPermissions)
		{
			_specialDocumentPermissionWidget.UpdateData(newUserPermissions);
		}

		private void OnUpdateEntityUserPermissionsAction(IList<UserPermissionNode> newUserPermissions)
		{
			_documentPermissionWidget.UpdateData(newUserPermissions);
		}

		private void OnUpdateWarehousePermissionsViewAction()
		{
			vboxWarehousePrivileges.Remove(_warehousePermissionsView);
			_warehousePermissionsView.Destroy();
			CreateWarehousePermissionsView();
		}

		private void CreateWarehousePermissionsView()
		{
			_warehousePermissionsView = _widgetResolver.Resolve(ViewModel.WarehousePermissionsViewModel);
			vboxWarehousePrivileges.Add(_warehousePermissionsView);
			_warehousePermissionsView.ShowAll();
		}
		
		private void ViewModelOnUpdateUserRolesForCurrentRoleAction()
		{
			var currentUserRole = ViewModel.Entity.CurrentUserRole;
			ycomboboxDefaultRole.SelectedItem = null;
			ycomboboxDefaultRole.SetRenderTextFunc<UserRole>(ur => ur.Name);
			if(!(currentUserRole is null))
			{
				ycomboboxDefaultRole.SelectedItem = currentUserRole;
			}
		}
		
		private void AddUserRoleToUserCommandOnCanExecuteChanged(object sender, EventArgs e)
		{
			buttonAddRole.Sensitive = ViewModel.AddUserRoleToUserCommand.CanExecute();
		}
		
		private void RemoveUserRoleCommandOnCanExecuteChanged(object sender, EventArgs e)
		{
			buttonRemoveRole.Sensitive = ViewModel.RemoveUserRoleCommand.CanExecute();
		}

		public override void Destroy()
		{
			ViewModel.UpdateEntityUserPermissionsAction -= OnUpdateEntityUserPermissionsAction;
			ViewModel.UpdateEntitySubdivisionForUserPermissionsAction -= OnUpdateEntitySubdivisionForUserPermissionsAction;
			ViewModel.UpdateWarehousePermissionsAction -= OnUpdateWarehousePermissionsViewAction;
			ViewModel.UpdateUserRolesForCurrentRoleAction -= ViewModelOnUpdateUserRolesForCurrentRoleAction;
			ViewModel.AddUserRoleToUserCommand.CanExecuteChanged -= AddUserRoleToUserCommandOnCanExecuteChanged;
			ViewModel.RemoveUserRoleCommand.CanExecuteChanged -= RemoveUserRoleCommandOnCanExecuteChanged;
			_documentPermissionWidget.Model.PermissionListViewModel.PermissionsList.ContentChanged -= ViewModel.UpdateChanges;
			if(_specialDocumentPermissionWidget.ViewModel != null)
			{
				_specialDocumentPermissionWidget.ViewModel.ObservablePermissionsList.ListContentChanged -= ViewModel.UpdateChanges;
			}
		}
	}
}
