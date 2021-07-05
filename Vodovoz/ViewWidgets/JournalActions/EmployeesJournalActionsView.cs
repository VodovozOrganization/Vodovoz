using Gtk;
using QS.Views;
using QS.Views.GtkUI;
using Vodovoz.Journals.JournalActionsViewModels;

namespace Vodovoz.ViewWidgets.JournalActions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeesJournalActionsView : ViewBase<EmployeesJournalActionsViewModel>
	{
		public EmployeesJournalActionsView(EmployeesJournalActionsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			CreateDefaultButtons();

			btnResetPassword.Clicked += (sender, e) => ViewModel.ResetPasswordCommand.Execute();

			btnResetPassword.Binding.AddBinding(ViewModel, vm => vm.CanResetPassword, w => w.Sensitive).InitializeFromSource();
		}

		private void CreateDefaultButtons()
		{
			Widget entitiesJournalActions = new EntitiesJournalActionsView(ViewModel);
			hboxDefaultBtns.Add(entitiesJournalActions);
			entitiesJournalActions.Show();
		}
	}
}
