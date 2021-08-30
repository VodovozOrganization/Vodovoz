using Gtk;
using QS.Project.Journal.Actions.Views;
using QS.Views;
using Vodovoz.Journals.JournalActionsViewModels;

namespace Vodovoz.ViewWidgets.JournalActions
{
	public partial class DistrictsSetJournalActionsView : ViewBase<DistrictsSetJournalActionsViewModel>
	{
		public DistrictsSetJournalActionsView(DistrictsSetJournalActionsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			CreateDefaultButtons();

			btnDistrictSetCopy.Clicked += (sender, args) => ViewModel.CopyDistrictSetCommand.Execute();
			ybtnUpdateOnlines.Clicked += (sender, args) => ViewModel.UpdateOnlinesCommand.Execute();
			
			ybtnUpdateOnlines.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OnlinesText, w => w.Label)
				.AddBinding(vm => vm.CanChangeOnlineDeliveriesToday, w => w.Visible)
				.InitializeFromSource();
		}

		private void CreateDefaultButtons()
		{
			Widget entitiesJournalActions = new EntitiesJournalActionsView(ViewModel);
			hboxDefaultBtns.Add(entitiesJournalActions);
			entitiesJournalActions.Show();
		}
	}
}
