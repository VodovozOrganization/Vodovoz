using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;

namespace Vodovoz.Cash.FinancialCategoriesGroups
{
	[ToolboxItem(true)]
	public partial class FinancialCategoriesJournalFilterView : FilterViewBase<FinancialCategoriesJournalFilterViewModel>
	{
		public FinancialCategoriesJournalFilterView(FinancialCategoriesJournalFilterViewModel viewModel)
			: base(viewModel)
		{
			Build();
		}
	}
}
