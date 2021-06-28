using QS.Views.GtkUI;
using Vodovoz.ViewModels;
using QS.Widgets.GtkUI;
using Vodovoz.Core.Permissions;

namespace Vodovoz.Views.Users
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserView : TabViewBase<UserViewModel>
	{
		public UserView(UserViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ConfigureDialog();
		}

		private void ConfigureDialog()
		{
			var presetPermissionWidget = new UserPresetPermissionWidget();
			var documentPermissionWidget = new UserEntityPermissionWidget();
			var specialDocumentPermissionWidget = new SubdivisionForUserEntityPermissionWidget();

			//Отключены, так как это простейший вариант диалога, вводимый из-за срочности ввода новых прав на склады
			tableRoles.Visible = false;
			ybuttonSetNewPassword.Visible = false;
			ybuttonResetPassword.Visible = false;
			PasswordWarning.Visible = false;
			ycheckRequirePasswordChange.Visible = false;

			ycheckIsAdmin.Binding.AddBinding(ViewModel.Entity, e => e.IsAdmin, w => w.Active).InitializeFromSource();
			ycheckUserDisabled.Binding.AddBinding(ViewModel.Entity, e => e.Deactivated, w => w.Active).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Description, w => w.Buffer.Text).InitializeFromSource();
			ylabelIdValue.Binding.AddBinding(ViewModel.Entity, e => e.Id, w => w.LabelProp).InitializeFromSource();
			ylabelDisplayName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.LabelProp).InitializeFromSource();
			ylabelLogin.Binding.AddBinding(ViewModel.Entity, e => e.Login, w => w.LabelProp).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => {
				presetPermissionWidget.Save();
				documentPermissionWidget.Save();
				specialDocumentPermissionWidget.Save();
				ViewModel.SaveCommand.Execute();
			};

			buttonCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();

			ybuttonPresetPrivileges.Sensitive = false;
			ybuttonDocumentPrivileges.Sensitive = false;
			ybuttonWarehousePrivileges.Sensitive = false;
			ybuttonSpecialDocumentPrivileges.Sensitive = false;

			if(ViewModel.Entity.Id != 0)
			{
				presetPermissionWidget.ConfigureDlg(ViewModel.UoW, ViewModel.Entity);
				vboxPresetPrivileges.Add(presetPermissionWidget);
				presetPermissionWidget.Show();
				ybuttonPresetPrivileges.Sensitive = true;

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
