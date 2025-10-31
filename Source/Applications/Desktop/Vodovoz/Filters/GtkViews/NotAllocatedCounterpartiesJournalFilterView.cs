using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Payments;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class NotAllocatedCounterpartiesJournalFilterView : FilterViewBase<NotAllocatedCounterpartiesJournalFilterViewModel>
	{
		public NotAllocatedCounterpartiesJournalFilterView(NotAllocatedCounterpartiesJournalFilterViewModel viewModel) : base(viewModel)
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
