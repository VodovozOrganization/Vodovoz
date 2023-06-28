using QS.Views.GtkUI;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.Filters.GtkViews
{
	public partial class UndeliveryDetalizationFilterView
		: FilterViewBase<UndeliveryDetalizationJournalFilterViewModel>
	{
		public UndeliveryDetalizationFilterView(
			UndeliveryDetalizationJournalFilterViewModel filterViewModel)
			: base(filterViewModel)
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
				.AddBinding(vm => vm.CanChangeUndeliveryObject, w => w.Sensitive)
				.InitializeFromSource();

			cmbUndeliveryKind.SetRenderTextFunc<UndeliveryKind>(k => k.GetFullName);
			cmbUndeliveryKind.ShowSpecialStateAll = true;
			cmbUndeliveryKind.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.VisibleUndeliveryKinds, w => w.ItemsList)
				.AddBinding(vm => vm.UndeliveryKind, w => w.SelectedItem)
				.AddBinding(vm => vm.CanChangeUndeliveryKind, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
