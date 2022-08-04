using QS.Views.GtkUI;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using QS.Widgets;

namespace Vodovoz.Filters.GtkViews
{
	public partial class FastDeliveryAvailabilityFilterView : FilterViewBase<FastDeliveryAvailabilityFilterViewModel>
	{
		public FastDeliveryAvailabilityFilterView(FastDeliveryAvailabilityFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.Counterparty, w => w.Subject).InitializeFromSource();

			entryLogistician.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			entryLogistician.Binding.AddBinding(ViewModel, vm => vm.Logistician, w => w.Subject).InitializeFromSource();

			entryDistrict.SetEntityAutocompleteSelectorFactory(ViewModel.DistrictSelectorFactory);
			entryDistrict.Binding.AddBinding(ViewModel, vm => vm.District, w => w.Subject).InitializeFromSource();

			entryReactionTime.Binding.AddBinding(ViewModel, vm => vm.LogisticianReactionTimeMinutes, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			ydateperiodpickerVerificationDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.VerificationDateFrom, w => w.StartDateOrNull)
				.AddBinding(ViewModel, vm => vm.VerificationDateTo, w => w.EndDateOrNull)
				.InitializeFromSource();

			nullablecheckVerificationSuccess.RenderMode = RenderMode.Icon;
			nullablecheckVerificationSuccess.Binding.AddBinding(ViewModel, vm => vm.IsValid, w => w.Active).InitializeFromSource();

			nullablecheckAssortmentStock.RenderMode = RenderMode.Icon;

			nullablecheckVerificatinFromSite.RenderMode = RenderMode.Icon;
		}
	}
}
