using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FinancialDistrictsSetsJournalFilterView : FilterViewBase<FinancialDistrictsSetsJournalFilterViewModel>
	{
		public FinancialDistrictsSetsJournalFilterView(
			FinancialDistrictsSetsJournalFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
