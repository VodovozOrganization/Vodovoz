using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class ProfitCategoriesJournalFilterView : FilterViewBase<ProfitCategoriesJournalFilterViewModel>
	{
		public ProfitCategoriesJournalFilterView(ProfitCategoriesJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			chkShowArchive.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchive, w => w.Active)
				.InitializeFromSource();
		}
	}
}
