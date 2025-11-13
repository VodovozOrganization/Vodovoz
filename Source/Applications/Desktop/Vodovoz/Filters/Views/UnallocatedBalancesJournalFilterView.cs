using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.Filters.Views
{
	[ToolboxItem(true)]
	public partial class UnallocatedBalancesJournalFilterView : FilterViewBase<UnallocatedBalancesJournalFilterViewModel>
	{
		public UnallocatedBalancesJournalFilterView(UnallocatedBalancesJournalFilterViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnHelp.Clicked += (sender, e) => ViewModel.HelpCommand.Execute();
			ConfigureEntries();
		}

		private void ConfigureEntries()
		{
			var builder = new LegacyEEVMBuilderFactory<UnallocatedBalancesJournalFilterViewModel>(
				ViewModel.JournalTab, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.Scope);

			counterpartyEntry.ViewModel = builder.ForProperty(x => x.Counterparty)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();
			
			organizationEntry.ViewModel = ViewModel.OrganizationViewModel;
		}
	}
}
