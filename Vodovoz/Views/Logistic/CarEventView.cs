using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalFilters;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class CarEventView : TabViewBase<CarEventViewModel>
	{
		public CarEventView(CarEventViewModel viewModel) :
			base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ylabelCreateDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.CreateDate.ToString("g"), w => w.LabelProp).InitializeFromSource();

			ylabelAuthor.Binding.AddFuncBinding(ViewModel.Entity, e => e.Author != null ? e.Author.GetPersonNameWithInitials() : "", w => w.LabelProp).InitializeFromSource();

			entityviewmodelentryCarEventType.SetEntityAutocompleteSelectorFactory(ViewModel.CarEventTypeSelectorFactory);
			entityviewmodelentryCarEventType.Binding.AddBinding(ViewModel.Entity, e => e.CarEventType, e => e.Subject).InitializeFromSource();

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(ViewModel.CarSelectorFactory);
			entityviewmodelentryCar.Binding.AddBinding(ViewModel.Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.ChangedByUser += (sender, e) => ViewModel.ChangeDriverCommand.Execute();

			var filterDriver = new EmployeeRepresentationFilterViewModel();
			filterDriver.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			referenceDriver.RepresentationModel = new EmployeesVM(filterDriver);
			referenceDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			ydatepickerStartEventDate.Binding.AddBinding(ViewModel.Entity, e => e.StartDate, w => w.Date).InitializeFromSource();

			ydatepickerEndEventDate.Binding.AddBinding(ViewModel.Entity, e => e.EndDate, w => w.Date).InitializeFromSource();

			ytextviewCommnet.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
