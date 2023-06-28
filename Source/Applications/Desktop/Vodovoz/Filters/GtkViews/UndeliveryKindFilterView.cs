using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.Filters.GtkViews
{
	public partial class UndeliveryKindFilterView : FilterViewBase<UndeliveryKindJournalFilterViewModel>
	{
		public UndeliveryKindFilterView(UndeliveryKindJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			cmbUndeliveryObject.ShowSpecialStateAll = true;
			cmbUndeliveryObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.UndeliveryObjects, w => w.ItemsList)
				.AddBinding(vm => vm.UndeliveryObject, w => w.SelectedItem)
				.InitializeFromSource();
		}
	}
}
