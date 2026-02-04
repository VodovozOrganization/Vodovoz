using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Banks;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class BanksJournalFilterView : FilterViewBase<BanksJournalFilterViewModel>
	{
		public BanksJournalFilterView(BanksJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}
