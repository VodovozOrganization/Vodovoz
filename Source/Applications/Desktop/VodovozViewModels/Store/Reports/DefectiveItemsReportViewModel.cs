using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Store.Reports
{
	public class DefectiveItemsReportViewModel : DialogViewModelBase, IDisposable
	{
		public Type DefectSourceType;
		private DefectiveItemsReport _report;
		private Employee _driver;
		private DateTime _startDate;
		private DateTime _endDate;
		private DefectSource? _defectSource;

		public DefectiveItemsReportViewModel(
			INavigationManager navigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			ViewModelEEVMBuilder<Employee> employeeViewModelEEVMBuilder)
			: base(navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			if(employeeViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(employeeViewModelEEVMBuilder));
			}

			Title = typeof(DefectiveItemsReport).GetClassUserFriendlyName().Nominative.CapitalizeSentence(); ;

			StartDate = DateTime.Today;
			EndDate = DateTime.Today;

			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot("Отчет по браку");

			DriverViewModel = employeeViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UnitOfWork)
				.ForProperty(this, x => x.Driver)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(x => x.Category = Core.Domain.Employees.EmployeeCategory.driver)
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DefectiveItemsReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		public Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		public DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public DefectSource? DefectSource
		{
			get => _defectSource;
			set => SetField(ref _defectSource, value);
		}

		public DefectSource[] HiddenDefectSources { get; }
		public IEntityEntryViewModel DriverViewModel { get; }
		public DelegateCommand GenerateReportCommand { get; }
		public IUnitOfWork UnitOfWork { get; private set; }

		private void GenerateReport()
		{
			Report = DefectiveItemsReport.Create(UnitOfWork, StartDate, EndDate, DefectSource, Driver?.Id);
		}

		public void Dispose()
		{
			UnitOfWork?.Dispose();
		}
	}
}
