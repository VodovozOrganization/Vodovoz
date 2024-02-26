using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.Filters.GtkViews
{
	public partial class CarEventFilterView : FilterViewBase<CarEventFilterViewModel>
	{
		public CarEventFilterView(CarEventFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ydateperiodpickerCreateEventDate.Binding.AddBinding(ViewModel, vm => vm.CreateEventDateFrom, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerCreateEventDate.Binding.AddBinding(ViewModel, vm => vm.CreateEventDateTo, w => w.EndDateOrNull).InitializeFromSource();

			ydateperiodpickerStartEventDate.Binding.AddBinding(ViewModel, vm => vm.StartEventDateFrom, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerStartEventDate.Binding.AddBinding(ViewModel, vm => vm.StartEventDateTo, w => w.EndDateOrNull).InitializeFromSource();

			ydateperiodpickerEndEventDate.Binding.AddBinding(ViewModel, vm => vm.EndEventDateFrom, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerEndEventDate.Binding.AddBinding(ViewModel, vm => vm.EndEventDateTo, w => w.EndDateOrNull).InitializeFromSource();

			evmeAuthor.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			evmeAuthor.Binding.AddBinding(ViewModel, vm => vm.Author, w => w.Subject).InitializeFromSource();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			evmeDriver.Binding.AddBinding(ViewModel, vm => vm.Driver, w => w.Subject).InitializeFromSource();

			entityentryCar.ViewModel = ViewModel.CarEntryViewModel;

			entityviewmodelentryCarEventType.SetEntityAutocompleteSelectorFactory(ViewModel.CarEventTypeSelectorFactory);
			entityviewmodelentryCarEventType.Binding.AddBinding(ViewModel, vm => vm.CarEventType, e => e.Subject).InitializeFromSource();
		}
	}
}
