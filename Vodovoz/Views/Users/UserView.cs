using QS.Views.GtkUI;
using Vodovoz.ViewModels;
using QS.Widgets.GtkUI;
using Vodovoz.Core.Permissions;
using Vodovoz.ViewWidgets.Permissions;
using Vodovoz.Core;
using Gtk;

namespace Vodovoz.Views.Users
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserView : TabViewBase<UserViewModel>
	{
		ViewModelWidgetResolver _widgetResolver = ViewModelWidgetResolver.Instance;

		public UserView(UserViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDialog();
		}

		private void ConfigureDialog()
		{
			var presetPermissionWidget = new PresetPermissionsView(ViewModel.PresetPermissionsViewModel);
			var documentPermissionWidget = new UserEntityPermissionWidget();
			var specialDocumentPermissionWidget = new SubdivisionForUserEntityPermissionWidget();

			//Отключены, так как это простейший вариант диалога, вводимый из-за срочности ввода новых прав на склады
			tableRoles.Visible = false;
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
				documentPermissionWidget.Save();
				specialDocumentPermissionWidget.Save();
				ViewModel.SaveCommand.Execute();
			};

			buttonCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();

			vboxPresetPrivileges.Add(presetPermissionWidget);
			presetPermissionWidget.Show();

			var warehousePermissionsView = _widgetResolver.Resolve(ViewModel.WarehousePermissionsViewModel);
			vboxWarehousePrivileges.Add(warehousePermissionsView);
			warehousePermissionsView.ShowAll();

			ybuttonDocumentPrivileges.Sensitive = false;
			ybuttonSpecialDocumentPrivileges.Sensitive = false;

			if(ViewModel.Entity.Id != 0)
			{
				documentPermissionWidget.ConfigureDlg(ViewModel.UoW, ViewModel.Entity);
				vboxDocumentPrivileges.Add(documentPermissionWidget);
				documentPermissionWidget.Show();
				ybuttonDocumentPrivileges.Sensitive = true;

				specialDocumentPermissionWidget.ConfigureDlg(ViewModel.UoW, ViewModel.Entity);
				vboxSpecialDocumentPrivileges.Add(specialDocumentPermissionWidget);
				specialDocumentPermissionWidget.Show();
				ybuttonSpecialDocumentPrivileges.Sensitive = true;
			}
		}
	}
}
