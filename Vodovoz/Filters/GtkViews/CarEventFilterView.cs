using System;
using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalFilters;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
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

			referenceAuthor.RepresentationModel = new EmployeesVM(new EmployeeRepresentationFilterViewModel());
			referenceAuthor.Binding.AddBinding(ViewModel, vm => vm.Author, w => w.Subject).InitializeFromSource();

			var filterDriver = new EmployeeRepresentationFilterViewModel();
			filterDriver.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			referenceDriver.RepresentationModel = new EmployeesVM(filterDriver);
			referenceDriver.Binding.AddBinding(ViewModel, vm => vm.Driver, w => w.Subject).InitializeFromSource();

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(ViewModel.CarSelectorFactory);
			entityviewmodelentryCar.Binding.AddBinding(ViewModel, vm => vm.Car, w => w.Subject).InitializeFromSource();

			entityviewmodelentryCarEventType.SetEntityAutocompleteSelectorFactory(ViewModel.CarEventTypeSelectorFactory);
			entityviewmodelentryCarEventType.Binding.AddBinding(ViewModel, vm => vm.CarEventType, e => e.Subject).InitializeFromSource();
		}
	}
}
