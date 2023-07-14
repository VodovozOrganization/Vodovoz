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
			var undeliveryObject = ViewModel.UndeliveryObject;
			var undeliveryKind = ViewModel.UndeliveryKind;

			cmbUndeliveryObject.ShowSpecialStateAll = true;
			cmbUndeliveryObject.SetRenderTextFunc<UndeliveryObject>(o => o.GetFullName);
			cmbUndeliveryObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.UndeliveryObject, w => w.SelectedItem)
				.AddBinding(vm => vm.UndeliveryObjects, w => w.ItemsList)
				.InitializeFromSource();

			cmbUndeliveryKind.SetRenderTextFunc<UndeliveryKind>(k => k.GetFullName);
			cmbUndeliveryKind.ShowSpecialStateAll = true;
			cmbUndeliveryKind.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.UndeliveryKind, w => w.SelectedItem)
				.AddBinding(vm => vm.UndeliveryKinds, w => w.ItemsList)
				.InitializeFromSource();

			ViewModel.UndeliveryObject = undeliveryObject;
			ViewModel.UndeliveryKind = undeliveryKind;
		}
	}
}
