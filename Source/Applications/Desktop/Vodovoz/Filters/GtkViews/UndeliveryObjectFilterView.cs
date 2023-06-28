using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.Filters.GtkViews
{
	public partial class UndeliveryObjectFilterView : FilterViewBase<UndeliveryObjectJournalFilterViewModel>
	{
		public UndeliveryObjectFilterView(UndeliveryObjectJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			chkIsArchive.Binding.AddBinding(ViewModel, vm => vm.IsArchive, w => w.Active).InitializeFromSource();
		}
	}
}
